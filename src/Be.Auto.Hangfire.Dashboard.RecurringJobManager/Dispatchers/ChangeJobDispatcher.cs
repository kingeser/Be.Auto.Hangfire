using Hangfire.Annotations;
using Hangfire.Dashboard;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Hangfire;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dispatchers
{
    internal sealed class ChangeJobDispatcher : IDashboardDispatcher
    {
        public async Task Dispatch([NotNull] DashboardContext context)
        {
            var response = new Response { Status = true };

            try
            {


                var job = CreatePeriodicJob(context);

                job.Register();

                context.Response.StatusCode = (int)HttpStatusCode.OK;

            }
            catch (Exception e)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                await WriteErrorResponse(context, response, e.GetAllMessages());
            }
            finally
            {
                await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
            }
        }

        private static PeriodicJob CreatePeriodicJob(DashboardContext context)
        {
            var jobType = (JobType)Enum.Parse(typeof(JobType), context.Request.GetQuery(nameof(PeriodicJob.JobType)));
            var methodParameters = context.Request.GetQuery(nameof(PeriodicJob.MethodParameters));
            return new PeriodicJob
            {
                Id = context.Request.GetQuery(nameof(PeriodicJob.Id)),
                Cron = context.Request.GetQuery(nameof(PeriodicJob.Cron)),
                Class = jobType == JobType.MethodCall ? context.Request.GetQuery(nameof(PeriodicJob.Class)) : context.Request.GetQuery("Domain"),
                Method = jobType == JobType.MethodCall ? context.Request.GetQuery(nameof(PeriodicJob.Method)) : context.Request.GetQuery("DomainPath"),
                TimeZoneId = context.Request.GetQuery(nameof(PeriodicJob.TimeZoneId)),
                JobType = jobType,
                BodyParameterType = (BodyParameterType)Enum.Parse(typeof(BodyParameterType), context.Request.GetQuery(nameof(PeriodicJob.BodyParameterType))),
                HttpMethod = (HttpMethodType)Enum.Parse(typeof(HttpMethodType), context.Request.GetQuery(nameof(PeriodicJob.HttpMethod))),
                BodyParameters = GetBodyParameters(context),
                HeaderParameters = GetHeaderParameters(context),
                MisfireHandlingMode = (MisfireHandlingMode)Enum.Parse(typeof(MisfireHandlingMode), context.Request.GetQuery(nameof(PeriodicJob.MisfireHandlingMode))),
                MethodParameters = JsonConvert.DeserializeObject<object[]>(methodParameters)

            };
        }

        private static async Task WriteErrorResponse(DashboardContext context, Response response, string message)
        {
            response.Status = false;
            response.Message = message;
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }


        private static List<HeaderParameter> GetHeaderParameters(DashboardContext context)
        {
            return GetParameters<HeaderParameter>(context, nameof(PeriodicJob.HeaderParameters));
        }

        private static List<BodyParameter> GetBodyParameters(DashboardContext context)
        {
            return GetParameters<BodyParameter>(context, nameof(PeriodicJob.BodyParameters));
        }

        private static List<T> GetParameters<T>(DashboardContext context, string parameterName) where T : new()
        {
            var result = new List<T>();

            var index = 0;

            while (true)
            {
                var name = context.Request.GetQuery($"{parameterName}[{index}].Name");

                var value = context.Request.GetQuery($"{parameterName}[{index}].Value");

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
                {
                    break;
                }

                var parameter = new T();
                var nameProperty = typeof(T).GetProperty("Name");
                var valueProperty = typeof(T).GetProperty("Value");

                nameProperty?.SetValue(parameter, name);
                valueProperty?.SetValue(parameter, value);

                result.Add(parameter);

                index++;
            }

            return result;
        }
    }
}
