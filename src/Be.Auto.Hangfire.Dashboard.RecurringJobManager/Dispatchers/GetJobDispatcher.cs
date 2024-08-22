using Hangfire.Annotations;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;
using Hangfire.Storage;
using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Dashboard;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dispatchers
{
    public sealed class GetJobDispatcher : IDashboardDispatcher
    {
        private readonly IStorageConnection _connection = JobStorage.Current.GetConnection();

        public async Task Dispatch([NotNull] DashboardContext context)
        {
            if (!"GET".Equals(context.Request.Method, StringComparison.InvariantCultureIgnoreCase))
            {
                context.Response.StatusCode = 405;

                return;
            }

            await context.Response.WriteAsync(RecurringJobAgent.GetAllJobs().SerializeObjectToJson());
        }
    }
}
