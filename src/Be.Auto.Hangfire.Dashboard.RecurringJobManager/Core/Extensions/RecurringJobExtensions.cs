using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hangfire.Common;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Hangfire;
using Cronos;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq.Expressions;


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


                        switch (webRequestJob.BodyParameterType)
                        {
                            case BodyParameterType.None:
                                break;
                            case BodyParameterType.Json:
                                {
                                    if (webRequestJob.BodyParameters.IsValidJson())
                                        throw new RecurringJobException("Job registration failed: The 'BodyParameters' field contains invalid JSON and cannot be processed.");

                                }

                                break;
                            case BodyParameterType.Xml:
                                {
                                    if (webRequestJob.BodyParameters.IsValidXml())
                                        throw new RecurringJobException("Job registration failed: The 'BodyParameters' field contains invalid XML and cannot be processed.");

                                }
                                break;
                            case BodyParameterType.FormUrlEncoded:
                                {
                                    if (!webRequestJob.BodyParameters.TryDeserializeObjectFromJson<List<HttpFormUrlEncodedParameter>>(out _))

                                    {
                                        throw new RecurringJobException("Job registration failed: The 'BodyParameters' field could not be deserialized into a valid list of 'HttpFormUrlEncodedParameter'.");
                                    }
                                }
                                break;
                            case BodyParameterType.FormData:
                                {

                                    if (!webRequestJob.BodyParameters.TryDeserializeObjectFromJson<List<HttpFormDataParameter>>(out _))

                                    {
                                        throw new RecurringJobException("Job registration failed: The 'BodyParameters' field could not be deserialized into a valid list of 'HttpFormDataParameter'.");
                                    }
                                }
                                break;
                            case BodyParameterType.PlainText:
                                {
                                    if (string.IsNullOrEmpty(webRequestJob.BodyParameters))
                                    {
                                        throw new RecurringJobException("Job registration failed: The 'BodyParameters' field cannot be null or empty.");
                                    }
                                }
                                break;
                        }


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


                            try
                            {

                                var defaultParameters = method.GetDefaultParameters(methodCallJob);

                                new global::Hangfire.RecurringJobManager(JobStorage.Current).AddOrUpdate(job.Id, new Job(method.DeclaringType,method, defaultParameters), job.Cron, new RecurringJobOptions()
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
