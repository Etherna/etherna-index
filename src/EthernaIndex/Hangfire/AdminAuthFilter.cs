using Hangfire.Dashboard;

namespace Etherna.EthernaIndex.Hangfire
{
    public class AdminAuthFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            return httpContext.User.Identity.IsAuthenticated;
        }
    }
}
