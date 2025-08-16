using Hangfire.Annotations;
using Hangfire.Dashboard;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hangfire;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dispatchers
{
    internal sealed class SaveJobDispatcher : IDashboardDispatcher
    {
        public async Task Dispatch([NotNull] DashboardContext context)
        {
            var response = new Response { Status = true };

            try
            {
                var job = await CreateRecurringJob(context);

                job.Register();

                response.Status = true;

                context.Response.StatusCode = (int)HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                response.Status = false;

                response.Message = e.GetAllMessages();

                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            }
            finally
            {
                await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
            }
        }

        private static async Task<RecurringJobBase> CreateRecurringJob(DashboardContext context)
        {
            var jobType = (JobType)Enum.Parse(typeof(JobType), (await context.Request.GetFormValuesAsync(nameof(RecurringJobBase.JobType))).First());

            return jobType switch
            {
                JobType.MethodCall => new RecurringJobMethodCall()
                {
                    Id = (await context.Request.GetFormValuesAsync(nameof(RecurringJobBase.Id))).First(),
                    Cron = (await context.Request.GetFormValuesAsync(nameof(RecurringJobBase.Cron))).First(),
                    Type = (await context.Request.GetFormValuesAsync(nameof(RecurringJobMethodCall.Type))).First(),
                    Method = (await context.Request.GetFormValuesAsync(nameof(RecurringJobMethodCall.Method))).First(),
                    TimeZoneId = (await context.Request.GetFormValuesAsync(nameof(RecurringJobBase.TimeZoneId))).First(),
                    MisfireHandlingMode =
                        (MisfireHandlingMode)Enum.Parse(typeof(MisfireHandlingMode),
                           (await context.Request.GetFormValuesAsync(nameof(RecurringJobBase.MisfireHandlingMode))).First()),
                    MethodParameters = (await context.Request.GetFormValuesAsync(nameof(RecurringJobMethodCall.MethodParameters))).First(),
                    LastJobState = string.Empty,
                    NextExecution = string.Empty,
                    CreatedAt = DateTime.Now.ToString("G"),
                    Error = string.Empty,
                    JobState = string.Empty,
                    Removed = false,
                    LastExecution = string.Empty,
                    LastJobId = string.Empty,
                    Guid = (await context.Request.GetFormValuesAsync(nameof(RecurringJobBase.Guid))).First(),
                    Job = null,
                    PreventConcurrentExecution = Convert.ToBoolean((await context.Request.GetFormValuesAsync(nameof(RecurringJobMethodCall.PreventConcurrentExecution))).First()),

                },
                JobType.WebRequest => new RecurringJobWebRequest()
                {
                    Id = (await context.Request.GetFormValuesAsync(nameof(RecurringJobBase.Id))).First(),
                    Cron = (await context.Request.GetFormValuesAsync(nameof(RecurringJobBase.Cron))).First(),
                    HostName = (await context.Request.GetFormValuesAsync(nameof(RecurringJobWebRequest.HostName))).First(),
                    UrlPath = (await context.Request.GetFormValuesAsync(nameof(RecurringJobWebRequest.UrlPath))).First(),
                    TimeZoneId = (await context.Request.GetFormValuesAsync(nameof(RecurringJobBase.TimeZoneId))).First(),
                    BodyParameterType =
                        (BodyParameterType)Enum.Parse(typeof(BodyParameterType),
                            (await context.Request.GetFormValuesAsync(nameof(RecurringJobWebRequest.BodyParameterType))).First()),
                    HttpMethod =
                        (HttpMethodType)Enum.Parse(typeof(HttpMethodType),
                            (await context.Request.GetFormValuesAsync(nameof(RecurringJobWebRequest.HttpMethod))).First()),
                    BodyParameters = (await context.Request.GetFormValuesAsync(nameof(RecurringJobWebRequest.BodyParameters))).First(),
                    HeaderParameters = (await context.Request.GetFormValuesAsync(nameof(RecurringJobWebRequest.HeaderParameters))).First(),
                    MisfireHandlingMode =
                        (MisfireHandlingMode)Enum.Parse(typeof(MisfireHandlingMode),
                            (await context.Request.GetFormValuesAsync(nameof(RecurringJobBase.MisfireHandlingMode))).First()),
                    LastJobState = string.Empty,
                    NextExecution = string.Empty,
                    CreatedAt = DateTime.Now.ToString("G"),
                    Error = string.Empty,
                    JobState = string.Empty,
                    Removed = false,
                    LastExecution = string.Empty,
                    LastJobId = string.Empty,
                    Guid = (await context.Request.GetFormValuesAsync(nameof(RecurringJobBase.Guid))).First(),
                    Job = null,
                    PreventConcurrentExecution = Convert.ToBoolean((await context.Request.GetFormValuesAsync(nameof(RecurringJobMethodCall.PreventConcurrentExecution))).First()),

                },
                _ => default
            };
        }



    }
}
