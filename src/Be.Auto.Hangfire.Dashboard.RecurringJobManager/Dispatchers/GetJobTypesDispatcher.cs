using System;
using System.Linq;

using System.Threading.Tasks;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;
using Hangfire.Annotations;
using Hangfire.Dashboard;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dispatchers
{
    internal sealed class GetJobTypesDispatcher : IDashboardDispatcher
    {
        public async Task Dispatch([NotNull] DashboardContext context)
        {

            var types = Enum.GetValues(typeof(JobType)).Cast<JobType>().ToList();

            if (AssemblyInfoStorage.Assemblies?.Count <= 0)
            {
                types.Remove(JobType.MethodCall);
            }

            await context.Response.WriteAsync((from a in types
                                               select new
                                               {
                                                   Text = a.GetDescription(),
                                                   Value = a.ToString()
                                               }).ToList().SerializeObjectToJson());
        }
    }
}