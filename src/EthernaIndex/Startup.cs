using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Persistence;
using Etherna.EthernaIndex.Services;
using Etherna.EthernaIndex.Services.Settings;
using Etherna.EthernaIndex.Swagger;
using Etherna.EthernaIndex.SystemStore;
using Etherna.MongODM;
using Etherna.MongODM.HF.Tasks;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
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
using System.Reflection;
using System.Text.Json.Serialization;

namespace Etherna.EthernaIndex
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
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
                    options.DefaultScheme = "Cookies";
                    options.DefaultChallengeScheme = "oidc";
                })
                .AddCookie("Cookies")
                .AddOpenIdConnect("oidc", options => //client config
                {
                    options.Authority = Configuration["SsoServer:BaseUrl"];

                    options.ClientId = "ethernaIndexClientId";
                    options.ClientSecret = Configuration["SsoServer:ClientSecret"];
                    options.ResponseType = "code";

                    options.SaveTokens = true;

                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("ether_accounts");
                });

            // Configure Hangfire services.
            services.AddHangfire(options =>
            {
                options.UseMongoStorage(
                    Configuration["ConnectionStrings:HangfireDb"],
                    new MongoStorageOptions
                    {
                        MigrationOptions = new MongoMigrationOptions //don't remove, could throw exception
                        {
                            MigrationStrategy = new MigrateMongoMigrationStrategy(),
                            BackupStrategy = new CollectionMongoBackupStrategy()
                        }
                    });
            });

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
            var appSettings = new ApplicationSettings
            {
                AssemblyVersion = GetType()
                    .GetTypeInfo()
                    .Assembly
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion!
            };
            services.Configure<ApplicationSettings>(options =>
            {
                options.AssemblyVersion = appSettings.AssemblyVersion;
            });
            services.Configure<SsoServerSettings>(Configuration.GetSection("SsoServer"));

            // Configure persistence.
            services.UseMongODM<HangfireTaskRunner, ModelBase>()
                .AddDbContext<IIndexContext, IndexContext>(options =>
                {
                    options.ApplicationVersion = appSettings.SimpleAssemblyVersion;
                    options.ConnectionString = Configuration["ConnectionStrings:IndexDb"];
                });

            // Configure domain services.
            services.AddDomainServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider apiProvider)
        {
            if (env.IsDevelopment())
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
                if (env.IsDevelopment())
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                }
                else
                {
                    builder.WithOrigins("https://*.etherna.io")
                           .SetIsOriginAllowedToAllowWildcardSubdomains()
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                }
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Add Hangfire.
            app.UseHangfireDashboard(
                "/admin/hangfire",
                new DashboardOptions
                {
                    AppPath = "/admin"
                });
            if (!env.IsStaging()) //don't init server in staging
                app.UseHangfireServer(new BackgroundJobServerOptions
                {
                    Queues = new[]
                    {
                        MongODM.Tasks.Queues.DB_MAINTENANCE,
                        "default"
                    },
                    WorkerCount = System.Environment.ProcessorCount * 2
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
