// Copyright 2021-present Etherna SA
// This file is part of Etherna Index.
// 
// Etherna Index is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Index is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Index.
// If not, see <https://www.gnu.org/licenses/>.

using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Elastic.Transport;
using Etherna.ACR.Exceptions;
using Etherna.ACR.Middlewares.DebugPages;
using Etherna.Authentication;
using Etherna.Authentication.AspNetCore;
using Etherna.Authentication.ClientCredentials;
using Etherna.DomainEvents;
using Etherna.EthernaIndex.Areas.Admin.Services;
using Etherna.EthernaIndex.Areas.Api;
using Etherna.EthernaIndex.Configs;
using Etherna.EthernaIndex.Configs.Authorization;
using Etherna.EthernaIndex.Configs.OpenApi;
using Etherna.EthernaIndex.Configs.Scrinium;
using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.ElasticSearch;
using Etherna.EthernaIndex.Extensions;
using Etherna.EthernaIndex.Persistence;
using Etherna.EthernaIndex.Services;
using Etherna.EthernaIndex.Services.Settings;
using Etherna.EthernaIndex.Services.Tasks;
using Etherna.Scrinium.AspNetCore.Extensions;
using Etherna.Scrinium.AspNetCore.UI;
using Etherna.Scrinium.Core.Options;
using Etherna.Scrinium.Extensions;
using Etherna.SwarmSdk.JsonConverters;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Debugging;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DashboardOptions = Etherna.Scrinium.AspNetCore.UI.DashboardOptions;
using IPNetwork = System.Net.IPNetwork;

namespace Etherna.EthernaIndex
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // Configure logging first.
            ConfigureLogging();

            // Then create the host, so that if the host fails we can log errors.
            try
            {
                Log.Information("Starting web host");

                var builder = WebApplication.CreateBuilder(args);

                // Configs.
                builder.Host.UseSerilog();
                builder.Host.UseDefaultServiceProvider(options =>
                {
                    // Db contexts are scoped: a singleton capturing one would silently pin its identity map
                    // for the process lifetime, so validate scopes in every environment.
                    options.ValidateScopes = true;
                });

                ConfigureServices(builder);

                var app = builder.Build();
                ConfigureApplication(app);

                // First operations.
                app.SeedDbContexts();
                app.CreateElasticIndexes();

                // Run application.
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        // Helpers.
        private static void ConfigureLogging()
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? throw new ServiceConfigurationException();
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var elasticNodes = (configuration.GetSection("Elastic:Urls").Get<string[]>() ?? throw new ServiceConfigurationException())
                .Select(u => new Uri(u))
                .ToArray();
            var elasticUsername = configuration["Elastic:Username"];
            var elasticPassword = configuration["Elastic:Password"];
            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name!.ToLower(CultureInfo.InvariantCulture).Replace(".", "-", StringComparison.InvariantCulture);
            var envName = env.ToLower(CultureInfo.InvariantCulture).Replace(".", "-", StringComparison.InvariantCulture);

            // The Elasticsearch sink reports its own failures (export exceptions, documents the cluster rejects)
            // only to Serilog's self log: show them on the console, or a dropped event leaves no trace.
            SelfLog.Enable(Console.Error);

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithIndexExceptionDetails()
                .Enrich.WithMachineName()
                .WriteTo.Debug(formatProvider: CultureInfo.InvariantCulture)
                .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
                .WriteTo.Elasticsearch(elasticNodes, opts =>
                {
                    opts.BootstrapMethod = BootstrapMethod.Silent;
                    opts.DataStream = new DataStreamName("logs", assemblyName, envName);
                }, transport =>
                {
                    // Apply basic auth only when credentials are configured, so the same build
                    // runs against both the unsecured cluster (no creds) and the secured one
                    // (Elastic:Username/Password set via env).
                    if (!string.IsNullOrEmpty(elasticUsername) && !string.IsNullOrEmpty(elasticPassword))
                        transport.Authentication(new BasicAuthentication(elasticUsername, elasticPassword));
                })
                .Enrich.WithProperty("Environment", env)
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            var services = builder.Services;
            var config = builder.Configuration;
            var env = builder.Environment;
            
            // Configure Asp.Net Core framework services.
            services.AddDataProtection()
                .PersistKeysToDbContext(new DbContextOptions
                {
                    ConnectionString = config["ConnectionStrings:DataProtectionDb"] ?? throw new ServiceConfigurationException()
                })
                .SetApplicationName(CommonConsts.SharedCookieApplicationName);

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.All;

                var knownNetworksConfig = config.GetSection("ForwardedHeaders:KnownNetworks");
                if (knownNetworksConfig.Exists())
                {
                    var networks = knownNetworksConfig.Get<string[]>()!.Select(address =>
                    {
                        var parts = address.Split('/');
                        if (parts.Length != 2)
                            throw new ServiceConfigurationException();

                        return new IPNetwork(
                            IPAddress.Parse(parts[0]),
                            int.Parse(parts[1], CultureInfo.InvariantCulture));
                    });

                    foreach (var network in networks)
                        options.KnownIPNetworks.Add(network);
                }
            });

            services.AddCors();
            services.AddOpenApi("index03", options =>
            {
                options.AddDocumentTransformer(new IndexDocumentTransformer(
                    config["SsoServer:BaseUrl"] ?? throw new ServiceConfigurationException()));
                options.AddDocumentTransformer<MetadataFilterDocumentTransformer<IndexApiMarker>>();

                options.AddOperationTransformer<ApiMethodNeedsAuthOperationTransformer>();
                options.AddOperationTransformer<DeprecatedOperationTransformer>();
                options.AddOperationTransformer<RemoveDefaultResponse200OperationTransformer>();
                options.AddOperationTransformer<IndexOperationTransformer>();
                
                options.AddSchemaTransformer(new SwarmModelsSchemaTransformer(true, true, true));
            });
            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeAreaFolder(
                    CommonConsts.AdminArea, "/", CommonConsts.RequireAdministratorRolePolicy);
            });
            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new EthAddressJsonConverter());
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.SerializerOptions.Converters.Add(new PostageBatchIdJsonConverter());
                options.SerializerOptions.Converters.Add(new SwarmAddressJsonConverter());
                options.SerializerOptions.Converters.Add(new SwarmReferenceJsonConverter());
                options.SerializerOptions.Converters.Add(new SwarmUriJsonConverter());

                options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

            // Configure authentication.
            var allowUnsafeAuthorityConnection = false;
            if (config["SsoServer:AllowUnsafeConnection"] is not null)
                allowUnsafeAuthorityConnection = bool.Parse(config["SsoServer:AllowUnsafeConnection"] ?? throw new ServiceConfigurationException());

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

                    if (env.IsProduction())
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
                    options.Authority = config["SsoServer:BaseUrl"] ?? throw new ServiceConfigurationException();

                    options.RequireHttpsMetadata = !allowUnsafeAuthorityConnection;
                })
                .AddPolicyScheme(CommonConsts.UserAuthenticationPolicyScheme, CommonConsts.UserAuthenticationPolicyScheme, options =>
                {
                    //runs on each request
                    options.ForwardDefaultSelector = context =>
                    {
                        //filter by auth type
                        string authorization = context.Request.Headers[HeaderNames.Authorization]!;
                        if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                            return CommonConsts.UserAuthenticationJwtScheme;

                        //otherwise always check for cookie auth
                        return CommonConsts.UserAuthenticationCookieScheme;
                    };
                })
                .AddEthernaOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    // Set properties.
                    options.Authority = config["SsoServer:BaseUrl"] ?? throw new ServiceConfigurationException();
                    options.ClientId = config["SsoServer:Clients:Webapp:ClientId"] ?? throw new ServiceConfigurationException();
                    options.ClientSecret = config["SsoServer:Clients:Webapp:Secret"] ?? throw new ServiceConfigurationException();

                    options.RequireHttpsMetadata = !allowUnsafeAuthorityConnection;
                    options.ResponseType = "code";
                    options.SaveTokens = true;

                    options.Scope.Add("ether_accounts");
                    options.Scope.Add("offline_access"); //permit user access token refresh
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
                    [
                        new DenyAnonymousAuthorizationRequirement(),
                        new DenyBannedAuthorizationRequirement()
                    ],
                    Array.Empty<string>());

                //other policies
                options.AddPolicy(CommonConsts.RequireAdministratorRolePolicy,
                    policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.AddRequirements(new DenyBannedAuthorizationRequirement());
                        policy.AddRequirements(new RequireRoleAuthorizationRequirement(
                            CommonConsts.AdministratorRoleName));
                    });

                options.AddPolicy(CommonConsts.RequireSuperModeratorRolePolicy,
                    policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.AddRequirements(new DenyBannedAuthorizationRequirement());
                        policy.AddRequirements(new RequireRoleAuthorizationRequirement(
                            CommonConsts.AdministratorRoleName));
                    });      
                
                options.AddPolicy(CommonConsts.UserInteractApiScopePolicy, policy =>
                {
                    policy.AuthenticationSchemes = [CommonConsts.UserAuthenticationJwtScheme];
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", EthernaScopes.UserApiIndex);
                    policy.AddRequirements(new DenyBannedAuthorizationRequirement());
                });
            });

            //requirement handlers
            services.AddScoped<IAuthorizationHandler, DenyBannedAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, RequireRoleAuthorizationHandler>();

            // Configure token management.
            //client credentials application authenticating the Index to the other Etherna services.
            //currently used for authenticated Gateway downloads: acquires a token (aud userApi, scope
            //userApi.gateway) from the SSO and attaches it as a bearer token on the named HttpClient
            //consumed by the SwarmClient, so non-offered content can be indexed.
            services.AddEthernaClientCredentials(
                    new Uri(config["SsoServer:BaseUrl"] ?? throw new ServiceConfigurationException()),
                    requireHttps: !allowUnsafeAuthorityConnection)
                .AddClient(
                    "ethernaServicesTokenClient",
                    config["SsoServer:Clients:Services:ClientId"] ?? throw new ServiceConfigurationException(),
                    config["SsoServer:Clients:Services:Secret"] ?? throw new ServiceConfigurationException(),
                    [EthernaScopes.UserApiGateway],
                    CommonConsts.GatewayHttpClientName,
                    httpClient => httpClient.Timeout = TimeSpan.FromMinutes(10)); //match SwarmClient's default timeout

            // Configure Hangfire server.
            if (!env.IsStaging()) //don't start server in staging
            {
                //open the domain events execution context in each job, like Scrinium does for its own
                GlobalJobFilters.Filters.Add(new Configs.Hangfire.DomainEventsExecutionContextFilter());

                //register hangfire server
                services.AddHangfireServer(options =>
                {
                    options.Queues =
                    [
                        Queues.DB_MAINTENANCE,
                        Queues.METADATA_VIDEO_VALIDATOR,
                        Queues.ELASTIC_SEARCH_MAINTENANCE,
                        "default"
                    ];
                    options.WorkerCount = Environment.ProcessorCount * 2;
                });
            }

            // Configure setting.
            services.Configure<SsoServerSettings>(config.GetSection("SsoServer"));
            
            // Configure api handler.
            services.AddScoped<IIndexApiHandler, IndexApiHandler>();

            // Configure admin diagnostics.
            services.AddScoped<IExternalServiceChecker, ExternalServiceChecker>();

            // Configure persistence.
            services.AddScriniumWithHangfire(configureHangfireOptions: options =>
            {
                options.ConnectionString = config["ConnectionStrings:HangfireDb"] ?? throw new ServiceConfigurationException();
                options.StorageOptions = new MongoStorageOptions
                {
                    MigrationOptions = new MongoMigrationOptions //don't remove, could throw exception
                    {
                        MigrationStrategy = new MigrateMongoMigrationStrategy(),
                        BackupStrategy = new CollectionMongoBackupStrategy()
                    }
                };
            }, configureScriniumOptions: options =>
            {
                options.DbMaintenanceQueueName = Queues.DB_MAINTENANCE;
            })
                .AddDbContext<IIndexDbContext, IndexDbContext>(
                sp =>
                {
                    var eventDispatcher = sp.GetRequiredService<IEventDispatcher>();
                    var logger = sp.GetRequiredService<ILogger<IndexDbContext>>();
                    return new IndexDbContext(eventDispatcher, logger);
                },
                options =>
                {
                    options.ConnectionString = config["ConnectionStrings:IndexDb"] ?? throw new ServiceConfigurationException();

                    //a summary member read without a preload is a defect, not a query
                    options.ImplicitLazyLoad = ReactionMode.Throw;
                })

                .AddDbContext<ISharedDbContext, SharedDbContext>(options =>
                {
                    options.ConnectionString = config["ConnectionStrings:ServiceSharedDb"] ?? throw new ServiceConfigurationException();
                    options.ImplicitLazyLoad = ReactionMode.Throw;

                    //the SSO owns this database: any write, index or migration from here is denied
                    options.IsReadOnly = true;
                });

            services.AddScriniumAdminDashboard(new DashboardOptions
            {
                AppPath = "/" + CommonConsts.AdminArea,
                AuthFilters = [new AdminAuthFilter()],
                BasePath = CommonConsts.DatabaseAdminPath
            });

            // Configure infrastructure.
            services.AddElasticSearchServices(opts =>
            {
                opts.IndexesPrefix = "index-main-";
                opts.Urls = config.GetSection("Elastic:Urls").Get<string[]>() ?? throw new ServiceConfigurationException();
                opts.Username = config["Elastic:Username"];
                opts.Password = config["Elastic:Password"];
            });

            // Configure domain services.
            services.AddDomainServices(config, CommonConsts.GatewayHttpClientName);
        }

        private static void ConfigureApplication(WebApplication app)
        {
            var config = app.Configuration;
            var env = app.Environment;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseForwardedHeaders();
                app.UseEthernaAcrDebugPages();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseForwardedHeaders();
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors(builder => builder.ConfigureIndexPolicy(config, env));

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Add api and pages.
            app.MapOpenApi();
            app.MapRazorPages();

            app.MapIndexApi();

            // Add Hangfire.
            app.UseHangfireDashboard(
                CommonConsts.HangfireAdminPath,
                new Hangfire.DashboardOptions
                {
                    AppPath = "/" + CommonConsts.AdminArea,
                    Authorization = [new Configs.Hangfire.AdminAuthFilter()]
                });

            // Add Scalar API Reference.
            app.MapScalarApiReference((options, httpContext) =>
            {
                options.WithTitle("Etherna Index API")
                    .WithOpenApiRoutePattern("/openapi/index03.json")
                    .DisableAgent()
                    .HideClientButton()
                    .HideDeveloperTools()
                    .AddPreferredSecuritySchemes("OAuth")
                    .AddAuthorizationCodeFlow("OAuth", flow =>
                    {
                        flow.ClientId = config["SsoServer:Clients:Scalar:ClientId"] ?? throw new ServiceConfigurationException();
                        flow.RedirectUri = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/scalar/index03";
                        flow.Pkce = Pkce.Sha256;
                        flow.SelectedScopes = ["openid", "profile", "ether_accounts", "role", "userApi.index"];
                    });
            });
        }
    }
}