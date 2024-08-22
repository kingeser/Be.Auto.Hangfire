using System.Linq;
using System.Threading.Tasks;
using Hangfire.Annotations;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;
using Hangfire.Dashboard;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dispatchers
{
    public sealed class GetCurrentAssemblyTypeMethodsDispatcher : IDashboardDispatcher
    {
        public async Task Dispatch([NotNull] DashboardContext context)
        {
            var methods = AssemblyInfoStorage.GetMethodsByType(context.Request.GetQuery("Type")).Select(t =>t.GenerateFullName());
            await context.Response.WriteAsync(methods.SerializeObjectToJson());
        }
    }
}