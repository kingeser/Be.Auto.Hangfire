﻿using Hangfire.Annotations;
using Hangfire.Dashboard;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Newtonsoft.Json;
using System;
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
                var job = CreateRecurringJob(context);

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

        private static RecurringJobBase CreateRecurringJob(DashboardContext context)
        {
            var jobType = (JobType)Enum.Parse(typeof(JobType), context.Request.GetQuery(nameof(RecurringJobBase.JobType)));

            return jobType switch
            {
                JobType.MethodCall => new RecurringJobMethodCall()
                {
                    Id = context.Request.GetQuery(nameof(RecurringJobBase.Id)),
                    Cron = context.Request.GetQuery(nameof(RecurringJobBase.Cron)),
                    Type = context.Request.GetQuery(nameof(RecurringJobMethodCall.Type)),
                    Method = context.Request.GetQuery(nameof(RecurringJobMethodCall.Method)),
                    TimeZoneId = context.Request.GetQuery(nameof(RecurringJobBase.TimeZoneId)),
                    MisfireHandlingMode =
                        (MisfireHandlingMode)Enum.Parse(typeof(MisfireHandlingMode),
                            context.Request.GetQuery(nameof(RecurringJobBase.MisfireHandlingMode))),
                    MethodParameters = context.Request.GetQuery(nameof(RecurringJobMethodCall.MethodParameters)),
                    LastJobState = string.Empty,
                    NextExecution = string.Empty,
                    CreatedAt = DateTime.Now.ToString("G"),
                    Error = string.Empty,
                    JobState = string.Empty,
                    Removed = false,
                    LastExecution = string.Empty,
                    LastJobId = string.Empty,
                    Guid = context.Request.GetQuery(nameof(RecurringJobBase.Guid)),
                },
                JobType.WebRequest => new RecurringJobWebRequest()
                {
                    Id = context.Request.GetQuery(nameof(RecurringJobBase.Id)),
                    Cron = context.Request.GetQuery(nameof(RecurringJobBase.Cron)),
                    HostName = context.Request.GetQuery(nameof(RecurringJobWebRequest.HostName)),
                    UrlPath = context.Request.GetQuery(nameof(RecurringJobWebRequest.UrlPath)),
                    TimeZoneId = context.Request.GetQuery(nameof(RecurringJobBase.TimeZoneId)),
                    BodyParameterType =
                        (BodyParameterType)Enum.Parse(typeof(BodyParameterType),
                            context.Request.GetQuery(nameof(RecurringJobWebRequest.BodyParameterType))),
                    HttpMethod =
                        (HttpMethodType)Enum.Parse(typeof(HttpMethodType),
                            context.Request.GetQuery(nameof(RecurringJobWebRequest.HttpMethod))),
                    BodyParameters = context.Request.GetQuery(nameof(RecurringJobWebRequest.BodyParameters)),
                    HeaderParameters = context.Request.GetQuery(nameof(RecurringJobWebRequest.HeaderParameters)),
                    MisfireHandlingMode =
                        (MisfireHandlingMode)Enum.Parse(typeof(MisfireHandlingMode),
                            context.Request.GetQuery(nameof(RecurringJobBase.MisfireHandlingMode))),
                    LastJobState = string.Empty,
                    NextExecution = string.Empty,
                    CreatedAt = DateTime.Now.ToString("G"),
                    Error = string.Empty,
                    JobState = string.Empty,
                    Removed = false,
                    LastExecution = string.Empty,
                    LastJobId = string.Empty,
                    Guid = context.Request.GetQuery(nameof(RecurringJobBase.Guid)),
                },
                _ => default
            };
        }



    }
}
