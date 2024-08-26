using System;
using System.Threading.Tasks;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;
using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dispatchers;

internal sealed class GetJobsCancelledDispatcher : IDashboardDispatcher
{

    public async Task Dispatch([NotNull] DashboardContext context)
    {
        if (!"GET".Equals(context.Request.Method, StringComparison.InvariantCultureIgnoreCase))
        {
            context.Response.StatusCode = 405;

            return;
        }

        await context.Response.WriteAsync(RecurringJobAgent.GetCancelledJobs().SerializeObjectToJson());
    }
}