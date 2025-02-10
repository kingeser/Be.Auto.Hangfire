using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire.Common;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Hangfire;
using Cronos;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions
{
    internal static class RecurringJobExtensions
    {
        public static void Register(this RecurringJobBase job)
        {

            if (string.IsNullOrEmpty(job.Id))
                throw new RecurringJobException("Job registration failed: The 'Id' field cannot be null or empty.");

            if (RecurringJobAgent.IsJobIdExist(job))
            {
                throw new RecurringJobException("Job registration failed: The Job Id already exists. Please provide a unique ID.");
            }

            if (string.IsNullOrEmpty(job.Cron))
                throw new RecurringJobException("Job registration failed: The 'Cron' expression cannot be null or empty.");

            if (string.IsNullOrEmpty(job.TimeZoneId))
                throw new RecurringJobException("Job registration failed: The 'TimeZoneId' field cannot be null or empty.");

            if (job.TimeZone == null)
                throw new RecurringJobException("Job registration failed: The specified 'TimeZone' could not be found. Please provide a valid time zone.");

            if (!CronExpression.TryParse(job.Cron, out _))
                throw new RecurringJobException($"Job registration failed: The provided Cron expression '{job.Cron}' is invalid. Please provide a valid Cron expression.");

            switch (job.JobType)
            {
                case JobType.WebRequest:
                    if (job is RecurringJobWebRequest webRequestJob)
                    {
                        if (string.IsNullOrEmpty(webRequestJob.HostName))
                            throw new RecurringJobException("Job registration failed: The 'HostName' field cannot be null or empty.");

                        if (string.IsNullOrEmpty(webRequestJob.UrlPath))
                            throw new RecurringJobException("Job registration failed: The 'UrlPath' field cannot be null or empty.");

                        switch (webRequestJob.BodyParameterType)
                        {
                            case BodyParameterType.None:
                                break;
                            case BodyParameterType.Json:
                                if (!webRequestJob.BodyParameters.IsValidJson())
                                    throw new RecurringJobException("Job registration failed: The 'BodyParameters' field contains invalid JSON.");

                                break;
                            case BodyParameterType.Xml:
                                if (!webRequestJob.BodyParameters.IsValidXml())
                                    throw new RecurringJobException("Job registration failed: The 'BodyParameters' field contains invalid XML.");

                                break;
                            case BodyParameterType.FormUrlEncoded:
                                if (!webRequestJob.BodyParameters.TryDeserializeObjectFromJson<List<HttpFormUrlEncodedParameter>>(out var formUrlEncodedParameters))
                                    throw new RecurringJobException("Job registration failed: The 'BodyParameters' field could not be deserialized into a valid list of 'HttpFormUrlEncodedParameter'.");

                                if (!formUrlEncodedParameters.Any())
                                    throw new RecurringJobException("Job registration failed: The 'BodyParameters' list is empty.");

                                if (formUrlEncodedParameters.Exists(t => string.IsNullOrEmpty(t.Name) || string.IsNullOrEmpty(t.Value)))
                                    throw new RecurringJobException("Job registration failed: The 'BodyParameters' list contains entries with empty 'Name' or 'Value' fields.");

                                break;
                            case BodyParameterType.FormData:
                                if (!webRequestJob.BodyParameters.TryDeserializeObjectFromJson<List<HttpFormDataParameter>>(out var formDataParameters))
                                    throw new RecurringJobException("Job registration failed: The 'BodyParameters' field could not be deserialized into a valid list of 'HttpFormDataParameter'.");

                                if (!formDataParameters.Any())
                                    throw new RecurringJobException("Job registration failed: The 'BodyParameters' list is empty.");

                                if (formDataParameters.Exists(t => string.IsNullOrEmpty(t.Name) || string.IsNullOrEmpty(t.Value) || string.IsNullOrEmpty(t.ContentType)))
                                    throw new RecurringJobException("Job registration failed: The 'BodyParameters' list contains entries with empty 'Name', 'Value', or 'ContentType' fields.");

                                break;
                            case BodyParameterType.PlainText:
                                if (string.IsNullOrEmpty(webRequestJob.BodyParameters))
                                    throw new RecurringJobException("Job registration failed: The 'BodyParameters' field cannot be null or empty.");

                                break;
                        }

                        try
                        {
                            if (string.IsNullOrEmpty(job.Guid))
                            {
                                job.Guid = Guid.NewGuid().ToString();
                            }

                            RemoveIfExist(job);

                            new global::Hangfire.RecurringJobManager(JobStorage.Current).AddOrUpdate(job.Id, new Job(typeof(RecurringJobWebClient), typeof(RecurringJobWebClient).GetMethod(nameof(RecurringJobWebClient.CallRequestAsync)), new WebRequestJob()
                            {
                                BodyParameters = webRequestJob.BodyParameters,
                                BodyParameterType = webRequestJob.BodyParameterType,
                                UrlPath = webRequestJob.UrlPath,
                                HeaderParameters = webRequestJob.HeaderParameters.UnescapeJson().DeserializeObjectFromJson<List<HttpHeaderParameter>>(),
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
                    if (job is RecurringJobMethodCall methodCallJob)
                    {
                        if (string.IsNullOrEmpty(methodCallJob.Method))
                            throw new RecurringJobException("Job registration failed: The 'Method' field cannot be null or empty.");

                        if (string.IsNullOrEmpty(methodCallJob.Type))
                            throw new RecurringJobException("Job registration failed: The 'Type' field cannot be null or empty.");

                        var type = AssemblyInfoStorage.GetType(methodCallJob);

                        if (type == null)
                            throw new RecurringJobException($"Job registration failed: The specified job type '{methodCallJob.Type}' could not be found. Please verify the type name is correct and available.");

                        var method = AssemblyInfoStorage.GetMethod(methodCallJob);

                        if (method == null)
                            throw new RecurringJobException($"Job registration failed: The specified method '{methodCallJob.Method}' could not be found in type '{methodCallJob.Type}'. Please ensure the method name is correct and exists.");

                        try
                        {
                            var parametersFromJob = method.GetDefaultParameters(methodCallJob);
                            var defaultParameters = method.GetDefaultParameters();

                            if (parametersFromJob.Length != defaultParameters.Length)
                                throw new RecurringJobException("Job registration failed: The number of parameters provided does not match the expected number of parameters.");



                            if (string.IsNullOrEmpty(job.Guid))
                            {
                                job.Guid = Guid.NewGuid().ToString();
                            }


                            RemoveIfExist(job);

                            new global::Hangfire.RecurringJobManager(JobStorage.Current).AddOrUpdate(job.Id, new Job(method.DeclaringType, method, parametersFromJob), job.Cron, new RecurringJobOptions()
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

                default:
                    throw new RecurringJobException($"Job registration failed: Unsupported job type '{job.JobType}'. Please provide a valid job type such as 'WebRequest' or 'MethodCall'.");
            }
        }

        private static void RemoveIfExist(RecurringJobBase job)
        {
            var result = RecurringJobAgent.GetJobIdWithGuid(job.Guid);

            if (result.Item1)
            {
                new global::Hangfire.RecurringJobManager(JobStorage.Current).RemoveIfExists(result.Item2);
            }

        }

    }
}
