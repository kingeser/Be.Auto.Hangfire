using System.Threading.Tasks;
using Hangfire.Annotations;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Hangfire.Dashboard;
using Newtonsoft.Json;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;
using Hangfire;
using Newtonsoft.Json.Schema;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dispatchers;

internal sealed class GetCurrentAssemblyTypeMethodJsonSchemaDispatcher : IDashboardDispatcher
{
    public async Task Dispatch([NotNull] DashboardContext context)
    {
        var type = context.Request.GetQuery(nameof(RecurringJobMethodCall.Class));
        var method = context.Request.GetQuery(nameof(RecurringJobMethodCall.Method));
        var parameters = AssemblyInfoStorage.GetMethod(type, method).GetParameterNamesAndDefaults();
        var json = JsonConvert.SerializeObject(parameters);
        var schema =  await NJsonSchema.JsonSchema.FromJsonAsync(json);
        var shemaString = schema.ToJson();
        await context.Response.WriteAsync(new
        {
            Json = json,
            Schema = shemaString
        }.SerializeObjectToJson());
    }
}