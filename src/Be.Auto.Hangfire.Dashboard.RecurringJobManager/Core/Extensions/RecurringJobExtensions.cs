using System;
using Hangfire.Common;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Hangfire;
using Cronos;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions
{
    public static class RecurringJobExtensions
    {
        public static void Register(this PeriodicJob job)
        {
            if (string.IsNullOrEmpty(job.Id))
                throw new RecurringJobException("Job registration failed: 'Id' field cannot be null or empty. Please provide a valid Job Id.");

            if (string.IsNullOrEmpty(job.Method))
                throw new RecurringJobException("Job registration failed: 'Method' field cannot be null or empty. Please provide the method name to execute.");

            if (string.IsNullOrEmpty(job.Cron))
                throw new RecurringJobException("Job registration failed: 'Cron' expression cannot be null or empty. Please provide a valid Cron expression.");

            if (job.TimeZone == null)
                throw new RecurringJobException("Job registration failed: 'TimeZone' field cannot be null. Please specify a valid time zone.");

            if (!CronExpression.TryParse(job.Cron, out _))
                throw new RecurringJobException($"Job registration failed: The provided Cron expression '{job.Cron}' is invalid. Please provide a valid Cron expression.");

            switch (job.JobType)
            {
                case JobType.WebRequest:
                    // WebRequest işleme kodu buraya eklenebilir
                    break;

                case JobType.MethodCall:
                    {
                        var type = AssemblyInfoStorage.GetType(job);

                        if (type == null)
                        {
                            throw new RecurringJobException($"Job registration failed: The specified job type '{job.Class}' could not be found in the assembly. Please check if the type name is correct and available.");
                        }

                        var method = AssemblyInfoStorage.GetMethod(job);

                        if (method == null)
                        {
                            throw new RecurringJobException($"Job registration failed: The specified method '{job.Method}' could not be found in type '{job.Class}'. Please ensure the method name is correct and exists.");
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
                    break;

                default:
                    throw new RecurringJobException($"Job registration failed: Unsupported job type '{job.JobType}'. Please provide a valid job type such as 'WebRequest' or 'MethodCall'.");
            }
        }
    }
}
