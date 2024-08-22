using System.Threading.Tasks;
using Hangfire.Annotations;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Hangfire.Dashboard;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dispatchers;

public sealed  class GetCurrentAssemblyTypeMethodJsonSchemaDispatcher : IDashboardDispatcher
{
    public async Task Dispatch([NotNull] DashboardContext context)
    {
        var type = context.Request.GetQuery(nameof(RecurringJobMethodCall.Type));
        var method = context.Request.GetQuery(nameof(RecurringJobMethodCall.Method));
        var schema = AssemblyInfoStorage.GetMethod(type, method)?.GetJsonSchema();

        await context.Response.WriteAsync(new
        {
            Json= schema?.ToSampleJson(),
            Schema = schema?.ToJson()
        }.SerializeObjectToJson());
    }
}