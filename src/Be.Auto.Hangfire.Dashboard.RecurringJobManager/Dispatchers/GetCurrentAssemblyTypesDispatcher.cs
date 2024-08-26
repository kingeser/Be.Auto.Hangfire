using System.Linq;
using System.Threading.Tasks;
using Hangfire.Annotations;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;
using Hangfire.Dashboard;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dispatchers
{
    internal sealed class GetCurrentAssemblyTypesDispatcher : IDashboardDispatcher
    {
        public async Task Dispatch([NotNull] DashboardContext context)
        {
            var types = AssemblyInfoStorage.GetTypes().OrderBy(t => t.FullName).Select(t => t.FullName);
            await context.Response.WriteAsync(types.SerializeObjectToJson());
        }
    }
}