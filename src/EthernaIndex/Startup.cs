//   Copyright 2021-present Etherna Sagl
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Etherna.DomainEvents;
using Etherna.EthernaIndex.Configs;
using Etherna.EthernaIndex.Configs.Hangfire;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Extensions;
using Etherna.EthernaIndex.Persistence;
using Etherna.EthernaIndex.Services;
using Etherna.EthernaIndex.Services.Interfaces;
using Etherna.EthernaIndex.Services.ModelValidators;
using Etherna.EthernaIndex.Services.Settings;
using Etherna.EthernaIndex.Services.Tasks;
using Etherna.EthernaIndex.Swagger;
using Etherna.MongODM;
using Etherna.MongODM.AspNetCore.UI;
using Etherna.MongODM.Core.Options;
using Etherna.SSL.Exceptions;
using EthernaIndex.Swarm;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex
{
    public class Startup
    {
        // Constructor.
        public Startup(
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        // Properties.
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // Methods.
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure Asp.Net Core framework services.
            services.AddDataProtection()
                .PersistKeysToDbContext(new DbContextOptions { ConnectionString = Configuration["ConnectionStrings:SystemDb"] });

            services.AddCors();
            services.AddRazorPages();
            services.AddControllers()
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
            });
            services.AddVersionedApiExplorer(options =>
            {
                // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                // note: the specified format code will format the version as "'v'major[.minor][-status]"
                options.GroupNameFormat = "'v'VVV";

                // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                // can also be used to control the format of the API version in route templates
                options.SubstituteApiVersionInUrl = true;
            });
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.Cookie.Name = Configuration["Application:CompactName"];
                    options.AccessDeniedPath = "/AccessDenied";

                    if (Environment.IsProduction())
                    {
                        options.Cookie.Domain = ".etherna.io";
                    }
                })
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options => //client config
                {
                    options.Authority = Configuration["SsoServer:BaseUrl"];

                    // Response 401 for unauthorized call on api.
                    options.Events.OnRedirectToIdentityProvider = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api", StringComparison.InvariantCulture))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            context.HandleResponse();
                        }
                        return Task.CompletedTask;
                    };

                    options.ClientId = Configuration["SsoServer:Clients:Webapp:ClientId"];
                    options.ClientSecret = Configuration["SsoServer:Clients:Webapp:Secret"];
                    options.ResponseType = "code";

                    options.SaveTokens = true;

                    options.Scope.Add("ether_accounts");
                    options.Scope.Add("role");
                });
            services.AddAuthorization(options =>
            {
                options.AddPolicy(CommonConsts.RequireAdministratorClaimPolicy,
                    policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.RequireClaim(ClaimTypes.Role, CommonConsts.AdministratorRoleName);
                    });
            });
            // Configure token management.
            services.AddAccessTokenManagement();

            // Configure Hangfire server.
            if (!Environment.IsStaging()) //don't start server in staging
            {
                //register hangfire server
                services.AddHangfireServer(options =>
                {
                    options.Queues = new[]
                    {
                        Queues.DB_MAINTENANCE,
                        "default"
                    };
                    options.WorkerCount = System.Environment.ProcessorCount * 2;
                });
            }

            // Configure Swagger services.
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen(options =>
            {
                //add a custom operation filter which sets default values
                options.OperationFilter<SwaggerDefaultValues>();

                //integrate xml comments
                var xmlFile = typeof(Startup).GetTypeInfo().Assembly.GetName().Name + ".xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            // Configure setting.
            var assemblyVersion = new AssemblyVersion(GetType().GetTypeInfo().Assembly);
            services.Configure<ApplicationSettings>(Configuration.GetSection("Application") ?? throw new ServiceConfigurationException());
            services.PostConfigure<ApplicationSettings>(options =>
            {
                options.AssemblyVersion = assemblyVersion.Version;
            });
            services.Configure<SsoServerSettings>(Configuration.GetSection("SsoServer"));

            // Configure persistence.
            services.AddMongODMWithHangfire(configureHangfireOptions: options =>
            {
                options.ConnectionString = Configuration["ConnectionStrings:HangfireDb"] ?? throw new ServiceConfigurationException();
                options.StorageOptions = new MongoStorageOptions
                {
                    MigrationOptions = new MongoMigrationOptions //don't remove, could throw exception
                    {
                        MigrationStrategy = new MigrateMongoMigrationStrategy(),
                        BackupStrategy = new CollectionMongoBackupStrategy()
                    }
                };
            }, configureMongODMOptions: options =>
            {
                options.DbMaintenanceQueueName = Queues.DB_MAINTENANCE;
            })
                .AddDbContext<IIndexContext, IndexContext>(
                sp =>
                {
                    var eventDispatcher = sp.GetRequiredService<IEventDispatcher>();
                    return new IndexContext(eventDispatcher);
                },
                options =>
                {
                    options.ConnectionString = Configuration["ConnectionStrings:IndexDb"] ?? throw new ServiceConfigurationException();
                    options.DocumentSemVer.CurrentVersion = assemblyVersion.SimpleVersion;
                });

            services.AddMongODMAdminDashboard(new MongODM.AspNetCore.UI.DashboardOptions
            {
                AuthFilters = new[] { new Configs.MongODM.AdminAuthFilter() },
                BasePath = CommonConsts.DatabaseAdminPath
            });

            // Configure domain services.
            services.AddDomainServices();

            // Configure Swarm service.
            services.AddTransient<ISwarmService, SwarmService>();

            // Configure Video serice.
            services.AddTransient<IMetadataVideoValidator, MetadataVideoValidator>();

            // AppSettings configurations.
            services.Configure<SwarmSettings>(Configuration.GetSection("Swarm"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IApiVersionDescriptionProvider apiProvider)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors(builder =>
            {
                if (Environment.IsDevelopment())
                {
                    builder.SetIsOriginAllowed(_ => true)
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials();
                }
                else
                {
                    builder.WithOrigins("https://*.etherna.io")
                           .SetIsOriginAllowedToAllowWildcardSubdomains()
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials();
                }
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Seed db if required.
            app.UseDbContextsSeeding();

            // Add Hangfire.
            app.UseHangfireDashboard(
                CommonConsts.HangfireAdminPath,
                new Hangfire.DashboardOptions
                {
                    Authorization = new[] { new AdminAuthFilter() }
                });

            // Add Swagger and SwaggerUI.
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                // build a swagger endpoint for each discovered API version
                foreach (var description in apiProvider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
            });

            // Add pages and controllers.
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
