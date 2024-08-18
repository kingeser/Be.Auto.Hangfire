using System;
using System.Threading.Tasks;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;
using Hangfire.Annotations;
using Hangfire.Dashboard;
using Newtonsoft.Json;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dispatchers
{
    internal sealed class GetHttpMethodsDispatcher : IDashboardDispatcher
    {
        public async Task Dispatch([NotNull] DashboardContext context)
        {
            await context.Response.WriteAsync(Enum.GetNames(typeof(HttpMethodType)).SerializeObjectToJson());
        }
    }
}