using Digicando.ExecContext.AsyncLocal;
using Digicando.MongODM;
using Digicando.MongODM.HF.Filters;
using Digicando.MongODM.HF.Tasks;
using Etherna.EthernaIndex.Domain;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Etherna.EthernaIndex.Persistence
{
    public static class ServiceCollectionExtensions
    {
        public static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            // MongODM.
            services.UseMongODM<HangfireTaskRunner>();
            services.UseMongODMDbContext<IIndexContext, IndexContext>();

            services.Configure<DbContextOptions>(nameof(IndexContext), opts =>
            {
                opts.ConnectionString = configuration["MONGODB_CONNECTIONSTRING"];
                opts.DBName = configuration["MONGODB_DBNAME"];
                opts.DocumentVersion = configuration["MONGODB_DOCUMENTVERSION"];
            });

            // Add Hangfire filters.
            GlobalJobFilters.Filters.Add(new AsyncLocalContextHangfireFilter(AsyncLocalContext.Instance));
        }
    }
}
