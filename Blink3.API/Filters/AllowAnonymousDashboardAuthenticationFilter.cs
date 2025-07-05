using Hangfire.Dashboard;

namespace Blink3.API.Filters;

public class AllowAnonymousDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // Allow all requests to access the dashboard
        return true;
    }
}