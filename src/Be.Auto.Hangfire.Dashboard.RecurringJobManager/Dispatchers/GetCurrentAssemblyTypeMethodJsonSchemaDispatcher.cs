using System;
using System.Threading.Tasks;
using Hangfire.Annotations;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Hangfire.Dashboard;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;
using Hangfire;

using System.Text.RegularExpressions;
using NJsonSchema.Generation;
using System.Reflection;
using NJsonSchema;

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using NJsonSchema.Infrastructure;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dispatchers;

internal sealed class GetCurrentAssemblyTypeMethodJsonSchemaDispatcher : IDashboardDispatcher
{
    public async Task Dispatch([NotNull] DashboardContext context)
    {
        var type = context.Request.GetQuery(nameof(RecurringJobMethodCall.Class));
        var method = context.Request.GetQuery(nameof(RecurringJobMethodCall.Method));
        var shemaString = AssemblyInfoStorage.GetMethod(type, method).GetJsonSchema();

        await context.Response.WriteAsync(new
        {
            Schema = shemaString
        }.SerializeObjectToJson());
    }
}