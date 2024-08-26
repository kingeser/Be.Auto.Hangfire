using Hangfire.Annotations;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;
using System;
using System.Threading.Tasks;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;
using Hangfire.Dashboard;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dispatchers
{
    internal sealed class GetJobsStoppedDispatcher : IDashboardDispatcher
    {

        public async Task Dispatch([NotNull] DashboardContext context)
        {
            if (!"GET".Equals(context.Request.Method, StringComparison.InvariantCultureIgnoreCase))
            {
                context.Response.StatusCode = 405;

                return;
            }

            await context.Response.WriteAsync(RecurringJobAgent.GetAllJobStopped().SerializeObjectToJson());
        }
    }
}
