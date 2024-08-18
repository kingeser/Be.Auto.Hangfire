using System.Linq;
using System.Threading.Tasks;
using Hangfire.Annotations;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;
using Hangfire.Dashboard;
using Newtonsoft.Json;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dispatchers
{
    internal sealed class GetCurrentAssemblyTypeMethodsDispatcher : IDashboardDispatcher
    {
        public async Task Dispatch([NotNull] DashboardContext context)
        {

            await context.Response.WriteAsync(AssemblyInfoStorage.GetMethodsByType(context.Request.GetQuery("Class")).Select(t => t.Name).SerializeObjectToJson());
        }
    }
}