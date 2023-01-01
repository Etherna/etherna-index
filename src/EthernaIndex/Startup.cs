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

using Etherna.ACR.Exceptions;
using Etherna.DomainEvents;
using Etherna.EthernaIndex.Configs;
using Etherna.EthernaIndex.Configs.Authorization;
using Etherna.EthernaIndex.Configs.Hangfire;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.ElasticSearch;
using Etherna.EthernaIndex.Extensions;
using Etherna.EthernaIndex.Persistence;
using Etherna.EthernaIndex.Services;
using Etherna.EthernaIndex.Services.Settings;
using Etherna.EthernaIndex.Services.Tasks;
using Etherna.EthernaIndex.Swagger;
using Etherna.EthernaIndex.Swarm;
using Etherna.MongODM;
using Etherna.MongODM.AspNetCore.UI;
using Etherna.MongODM.Core.Options;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
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
                .PersistKeysToDbContext(new DbContextOptions
                {
                    ConnectionString = Configuration["ConnectionStrings:DataProtectionDb"] ?? throw new ServiceConfigurationException()
                })
                .SetApplicationName(CommonConsts.SharedCookieApplicationName);

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.All;

                var knownNetworksConfig = Configuration.GetSection("ForwardedHeaders:KnownNetworks");
                if (knownNetworksConfig.Exists())
                {
                    var networks = knownNetworksConfig.Get<string[]>().Select(address =>
                    {
                        var parts = address.Split('/');
                        if (parts.Length != 2)
                            throw new ServiceConfigurationException();

                        return new IPNetwork(
                            IPAddress.Parse(parts[0]),
                            int.Parse(parts[1], CultureInfo.InvariantCulture));
                    });

                    foreach (var network in networks)
                        options.KnownNetworks.Add(network);
                }
            });

            services.AddCors();
            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeAreaFolder(CommonConsts.AdminArea, "/", CommonConsts.RequireAdministratorClaimPolicy);
            });
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

            // Configure authentication.
            var allowUnsafeAuthorityConnection = false;
            if (Configuration["SsoServer:AllowUnsafeConnection"] is not null)
                allowUnsafeAuthorityConnection = bool.Parse(Configuration["SsoServer:AllowUnsafeConnection"]);

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CommonConsts.UserAuthenticationPolicyScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })

                //users access
                .AddCookie(CommonConsts.UserAuthenticationCookieScheme, options =>
                {
                    // Set properties.
                    options.AccessDeniedPath = "/AccessDenied";
                    options.Cookie.MaxAge = TimeSpan.FromDays(30);
                    options.Cookie.Name = CommonConsts.SharedCookieApplicationName;

                    if (Environment.IsProduction())
                        options.Cookie.Domain = ".etherna.io";

                    // Handle unauthorized call on api with 401 response. For already logged in users.
                    options.Events.OnRedirectToAccessDenied = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api", StringComparison.InvariantCulture))
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        else
                            context.Response.Redirect(context.RedirectUri);
                        return Task.CompletedTask;
                    };
                })
                .AddJwtBearer(CommonConsts.UserAuthenticationJwtScheme, options =>
                {
                    options.Audience = "userApi";
                    options.Authority = Configuration["SsoServer:BaseUrl"] ?? throw new ServiceConfigurationException();

                    options.RequireHttpsMetadata = !allowUnsafeAuthorityConnection;
                })
                .AddPolicyScheme(CommonConsts.UserAuthenticationPolicyScheme, CommonConsts.UserAuthenticationPolicyScheme, options =>
                {
                    //runs on each request
                    options.ForwardDefaultSelector = context =>
                    {
                        //filter by auth type
                        string authorization = context.Request.Headers[HeaderNames.Authorization];
                        if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                            return CommonConsts.UserAuthenticationJwtScheme;

                        //otherwise always check for cookie auth
                        return CommonConsts.UserAuthenticationCookieScheme;
                    };
                })
                .AddEthernaOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    // Set properties.
                    options.Authority = Configuration["SsoServer:BaseUrl"] ?? throw new ServiceConfigurationException();
                    options.ClientId = Configuration["SsoServer:Clients:Webapp:ClientId"] ?? throw new ServiceConfigurationException();
                    options.ClientSecret = Configuration["SsoServer:Clients:Webapp:Secret"] ?? throw new ServiceConfigurationException();

                    options.RequireHttpsMetadata = !allowUnsafeAuthorityConnection;
                    options.ResponseType = "code";
                    options.SaveTokens = true;

                    options.Scope.Add("ether_accounts");
                    options.Scope.Add("role");

                    // Handle unauthorized call on api with 401 response. For users not logged in.
                    options.Events.OnRedirectToIdentityProvider = context =>
                    {
                        if (context.Request.Path.StartsWithSegments("/api", StringComparison.InvariantCulture))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            context.HandleResponse();
                        }
                        return Task.CompletedTask;
                    };
                });

            // Configure authorization.
            //policy and requirements
            services.AddAuthorization(options =>
            {
                //default policy
                options.DefaultPolicy = new AuthorizationPolicy(
                    new IAuthorizationRequirement[]
                    {
                        new DenyAnonymousAuthorizationRequirement(),
                        new DenyBannedAuthorizationRequirement()
                    },
                    Array.Empty<string>());

                //other policies
                options.AddPolicy(CommonConsts.RequireAdministratorClaimPolicy,
                    policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.RequireClaim(ClaimTypes.Role, CommonConsts.AdministratorRoleName);
                    });

                options.AddPolicy(CommonConsts.RequireSuperModeratorClaimPolicy,
                    policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.RequireClaim(ClaimTypes.Role, CommonConsts.AdministratorRoleName);
                    });
            });

            //requirement handlers
            services.AddScoped<IAuthorizationHandler, DenyBannedAuthorizationHandler>();

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
                        Queues.METADATA_VIDEO_VALIDATOR,
                        Queues.ELASTIC_SEARCH_MAINTENANCE,
                        "default"
                    };
                    options.WorkerCount = System.Environment.ProcessorCount * 2;
                });
            }

            // Configure Swagger services.
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen(options =>
            {
                options.SupportNonNullableReferenceTypes();

                //add a custom operation filter which sets default values
                options.OperationFilter<SwaggerDefaultValues>();

                //integrate xml comments
                var xmlFile = typeof(Startup).GetTypeInfo().Assembly.GetName().Name + ".xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            // Configure setting.
            var assemblyVersion = new AssemblyVersion(GetType().GetTypeInfo().Assembly);
            services.Configure<ApplicationSettings>(Configuration.GetSection("Application"));
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
                .AddDbContext<IIndexDbContext, IndexDbContext>(
                sp =>
                {
                    var eventDispatcher = sp.GetRequiredService<IEventDispatcher>();
                    return new IndexDbContext(eventDispatcher);
                },
                options =>
                {
                    options.ConnectionString = Configuration["ConnectionStrings:IndexDb"] ?? throw new ServiceConfigurationException();
                })

                .AddDbContext<ISharedDbContext, SharedDbContext>(options =>
                {
                    options.ConnectionString = Configuration["ConnectionStrings:ServiceSharedDb"] ?? throw new ServiceConfigurationException();
                });

            services.AddMongODMAdminDashboard(new MongODM.AspNetCore.UI.DashboardOptions
            {
                AuthFilters = new[] { new Configs.MongODM.AdminAuthFilter() },
                BasePath = CommonConsts.DatabaseAdminPath
            });

            // Configure infrastructure.
            services.AddSwarmServices(Configuration);
            services.AddElasticSearchServices(Configuration.GetSection("Elastic:Urls").Get<string[]>(), opts =>
            {
                opts.IndexesPrefix = "etherna-mainindex-";
            });

            // Configure domain services.
            services.AddDomainServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IApiVersionDescriptionProvider apiProvider)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseForwardedHeaders();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseForwardedHeaders();
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
                    builder.WithOrigins("https://etherna.io")
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
                options.DocumentTitle = "Etherna Index API";

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

            // Seed db.
            app.SeedDbContexts();
        }
    }
}
