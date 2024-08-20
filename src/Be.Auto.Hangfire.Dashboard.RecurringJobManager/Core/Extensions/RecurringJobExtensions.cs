using System;
using System.Collections.Generic;
using Hangfire.Common;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Hangfire;
using Cronos;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;


namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions
{
    public static class RecurringJobExtensions
    {
        public static void Register(this RecurringJobBase job)
        {
            if (string.IsNullOrEmpty(job.Id))
                throw new RecurringJobException("Job registration failed: 'Id' field cannot be null or empty.");


            if (string.IsNullOrEmpty(job.Cron))
                throw new RecurringJobException("Job registration failed: 'Cron' expression cannot be null or empty.");


            if (string.IsNullOrEmpty(job.TimeZoneId))
                throw new RecurringJobException("Job registration failed: 'TimeZoneId' field cannot be null or empty.");


            if (job.TimeZone == null)
                throw new RecurringJobException("Job registration failed: 'TimeZone' field cannot be found. Please specify a valid time zone.");

            if (!CronExpression.TryParse(job.Cron, out _))
                throw new RecurringJobException($"Job registration failed: The provided Cron expression '{job.Cron}' is invalid. Please provide a valid Cron expression.");

            switch (job.JobType)
            {
                case JobType.WebRequest:
                    if (job is RecurringJobWebRequest webRequestJob)
                    {
                        if (string.IsNullOrEmpty(webRequestJob.HostName))
                            throw new RecurringJobException("Job registration failed: 'HostName' field cannot be null or empty.");

                        if (string.IsNullOrEmpty(webRequestJob.UrlPath))
                            throw new RecurringJobException("Job registration failed: 'UrlPath' field cannot be null or empty.");

                        try
                        {

                            new global::Hangfire.RecurringJobManager(JobStorage.Current).AddOrUpdate(job.Id, new Job(typeof(RecurringJobWebClient), typeof(RecurringJobWebClient).GetMethod(nameof(RecurringJobWebClient.CallRequestAsync)), new WebRequestJob()
                            {
                                BodyParameters = webRequestJob.BodyParameters,
                                BodyParameterType = webRequestJob.BodyParameterType,
                                UrlPath = webRequestJob.UrlPath,
                                HeaderParameters = webRequestJob.HeaderParameters.DeserializeObjectFromJson<List<HttpHeaderParameter>>(),
                                HostName = webRequestJob.HostName,
                                HttpMethod = webRequestJob.HttpMethod,
                                
                            }), job.Cron, new RecurringJobOptions()
                            {
                                TimeZone = job.TimeZone,
                                MisfireHandling = job.MisfireHandlingMode,
                            });

                            RecurringJobAgent.SaveJobDetails(job);
                        }
                        catch (Exception e)
                        {
                            throw new RecurringJobException("Job registration failed: An unexpected error occurred while registering the job. See the inner exception for more details.", e);
                        }
                    }
                    break;

                case JobType.MethodCall:
                    {

                        if (job is RecurringJobMethodCall methodCallJob)
                        {
                            if (string.IsNullOrEmpty(methodCallJob.Method))
                                throw new RecurringJobException("Job registration failed: 'Method' field cannot be null or empty.");

                            if (string.IsNullOrEmpty(methodCallJob.Class))
                                throw new RecurringJobException("Job registration failed: 'Class' field cannot be null or empty.");

                            var type = AssemblyInfoStorage.GetType(methodCallJob);

                            if (type == null)
                            {
                                throw new RecurringJobException($"Job registration failed: The specified job type '{methodCallJob.Class}' could not be found in the assembly. Please check if the type name is correct and available.");
                            }

                            var method = AssemblyInfoStorage.GetMethod(methodCallJob);

                            if (method == null)
                            {
                                throw new RecurringJobException($"Job registration failed: The specified method '{methodCallJob.Method}' could not be found in type '{methodCallJob.Class}'. Please ensure the method name is correct and exists.");
                            }

                            var defaultParameters = method.GetDefaultParameters();

                            try
                            {
                                new global::Hangfire.RecurringJobManager(JobStorage.Current).AddOrUpdate(job.Id, new Job(type, method, defaultParameters), job.Cron, new RecurringJobOptions()
                                {
                                    TimeZone = job.TimeZone,
                                    MisfireHandling = job.MisfireHandlingMode,
                                });

                                RecurringJobAgent.SaveJobDetails(job);
                            }
                            catch (Exception e)
                            {
                                throw new RecurringJobException("Job registration failed: An unexpected error occurred while registering the job. See the inner exception for more details.", e);
                            }
                        }

                    }
                    break;

                default:
                    throw new RecurringJobException($"Job registration failed: Unsupported job type '{job.JobType}'. Please provide a valid job type such as 'WebRequest' or 'MethodCall'.");
            }
        }
    }
}
