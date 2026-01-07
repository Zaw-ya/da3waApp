using Hangfire.Dashboard;

namespace Da3wa.WebUI.Filters;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Allow authenticated users only
        return httpContext.User.Identity?.IsAuthenticated ?? false;
    }
}
