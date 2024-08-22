using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Annotations;
using Hangfire.Dashboard;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dispatchers
{
    public sealed  class GetBodyTypesDispatcher : IDashboardDispatcher
    {
        public async Task Dispatch([NotNull] DashboardContext context)
        {
            Enum.TryParse<HttpMethodType>(context.Request.GetQuery("HttpMethod"), out var httpMethodType);

            var bodyParameters = Enum.GetValues(typeof(BodyParameterType)).Cast<BodyParameterType>();

            switch (httpMethodType)
            {
                case HttpMethodType.GET:
                case HttpMethodType.DELETE:
                case HttpMethodType.HEAD:
                case HttpMethodType.OPTIONS:
                case HttpMethodType.TRACE:
                    bodyParameters = new[] { BodyParameterType.None, BodyParameterType.FormUrlEncoded };
                    break;
                case HttpMethodType.POST:
                case HttpMethodType.PUT:
                case HttpMethodType.PATCH:
                    bodyParameters = bodyParameters.Where(t => t != BodyParameterType.FormUrlEncoded);
                    break;

            }

            await context.Response.WriteAsync((from a in bodyParameters
                                               select new
                                               {
                                                   Text = a.GetDescription(),
                                                   Value = a.ToString()
                                               }).ToList().SerializeObjectToJson());
        }
    }
}