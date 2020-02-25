using Etherna.EthernaIndex.ApiApplication.V1;
using Etherna.EthernaIndex.Persistence;
using Etherna.EthernaIndex.Services;
using Hangfire;
using Hangfire.Mongo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;

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
            services.AddControllers();

            // Add Hangfire services.
            services.AddHangfire(config =>
            {
                config.UseMongoStorage(
                    Configuration["HANGFIRE_CONNECTIONSTRING"],
                    new MongoStorageOptions
                    {
                        MigrationOptions = new MongoMigrationOptions
                        {
                            Strategy = MongoMigrationStrategy.Migrate,
                            BackupStrategy = MongoBackupStrategy.Collections
                        }
                    });
            });

            // Add application services.
            var softwareVersion = typeof(Startup)
                .GetTypeInfo()
                .Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;

            Configuration["API_VERSION"] = softwareVersion;
            Configuration["MONGODB_DOCUMENTVERSION"] = softwareVersion;
            
            services.AddPersistence(Configuration);
            services.AddApiV1Application();
            services.AddDomainServices();

            // Set Swagger generation services.
            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Etherna Index API",
                    Version = Configuration["API_VERSION"]
                });
                config.CustomSchemaIds(sid => sid.Name);

                var xmlFile = $"{typeof(ApiApplication.V1.ServiceCollectionExtensions).Assembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                config.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

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
                        Digicando.MongODM.Tasks.Queues.DB_MAINTENANCE,
                        "default"
                    },
                    WorkerCount = System.Environment.ProcessorCount * 2
                });

            // Setup Swagger and SwaggerUI.
            app.UseSwagger();
            app.UseSwaggerUI(config =>
            {
                config.SwaggerEndpoint("/swagger/v1/swagger.json", "Etherna Index API");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
