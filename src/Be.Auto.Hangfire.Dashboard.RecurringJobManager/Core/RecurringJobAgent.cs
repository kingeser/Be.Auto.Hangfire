using Hangfire.Common;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Hangfire.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;
using Hangfire;
using Newtonsoft.Json;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core
{
    public static class RecurringJobAgent
    {
        public const string TagRecurringJob = "recurring-job";
        public const string TagStopJob = "recurring-jobs-stop";
        public static void StartBackgroundJob(string jobId)
        {
            using var connection = JobStorage.Current.GetConnection();
            using var transaction = connection.CreateWriteTransaction();
            transaction.RemoveFromSet(TagStopJob, jobId);
            transaction.AddToSet($"{TagRecurringJob}s", jobId);
            transaction.Commit();
        }
        public static void StopBackgroundJob(string jobId)
        {
            using var connection = JobStorage.Current.GetConnection();
            using var transaction = connection.CreateWriteTransaction();
            transaction.RemoveFromSet($"{TagRecurringJob}s", jobId);
            transaction.AddToSet($"{TagStopJob}", jobId);
            transaction.Commit();
        }
        public static void SaveJobDetails(PeriodicJob job)
        {
            using var connection = JobStorage.Current.GetConnection();
            using var transaction = connection.CreateWriteTransaction();
            transaction.SetRangeInHash($"{TagRecurringJob}:{job.Id}", new KeyValuePair<string, string>[]
            {
                new(nameof(job.JobType), job.JobType.ToString()),
                new(nameof(job.HttpMethod), job.HttpMethod.ToString()),
                new(nameof(job.BodyParameterType), job.BodyParameterType.ToString()),
                new(nameof(job.MisfireHandlingMode), job.MisfireHandlingMode.ToString()),
                new(nameof(job.BodyParameters), JsonConvert.SerializeObject(job.BodyParameters)),
                new(nameof(job.HeaderParameters), JsonConvert.SerializeObject(job.HeaderParameters)),
                new(nameof(job.MethodParameters), JsonConvert.SerializeObject(job.MethodParameters)),

            });

            transaction.Commit();
        }

        public static List<PeriodicJob> GetAllJobStopped() => GetAllJobs().Where(t => t.JobState == "Stopped").ToList();

        public static PeriodicJob GetJob(string jobId) => GetAllJobs().Find(t => t.Id == jobId);
        public static List<PeriodicJob> GetAllJobs()
        {
            var result = new List<PeriodicJob>();

            using var connection = JobStorage.Current.GetConnection();

            var runnigJobs = connection.GetRecurringJobs();

            runnigJobs.ForEach(recurringJobDto =>
            {
                var dto = MapPeriodicJob(connection, recurringJobDto.Id, "Running");
                if (dto == null) return;
                result.Add(dto);
            });

            var allJobStopped = connection.GetAllItemsFromSet(TagStopJob).ToList();

            allJobStopped.ForEach(jobId =>
            {
                var dto = MapPeriodicJob(connection, jobId, "Stopped");
                if (dto == null) return;
                result.Add(dto);
            });
            return result;
        }

        private static PeriodicJob MapPeriodicJob(IStorageConnection connection, string jobId, string status)
        {
            var dataJob = connection.GetAllEntriesFromHash($"{TagRecurringJob}:{jobId}");

            if (dataJob == null) return default;

            var dto = dataJob.BindFromDictionary<PeriodicJob>();


            dto.Id = jobId;

            dto.TimeZoneId = "UTC";

            try
            {
                if (dataJob.TryGetValue("Job", out var payload) && !string.IsNullOrWhiteSpace(payload))
                {
                    var invocationData = InvocationData.DeserializePayload(payload);
                    var job = invocationData.DeserializeJob();
                    dto.Method = job.Method.Name;
                    dto.Class = job.Type.Name;

                }
            }
            catch (JobLoadException ex)
            {
                dto.Error = ex.Message;
            }

            if (dataJob.TryGetValue("TimeZoneId", out var value))
            {
                dto.TimeZoneId = value;
            }

            if (dataJob.TryGetValue("NextExecution", out var value1))
            {
                var tempNextExecution = JobHelper.DeserializeNullableDateTime(value1);

                dto.NextExecution = tempNextExecution.HasValue ? tempNextExecution.Value.ChangeTimeZone(dto.TimeZoneId).ToString("G") : "N/A";
            }

            if (dataJob.ContainsKey("LastJobId") && !string.IsNullOrWhiteSpace(dataJob["LastJobId"]))
            {
                dto.LastJobId = dataJob["LastJobId"];

                var stateData = connection.GetStateData(dto.LastJobId);

                if (stateData != null)
                {
                    dto.LastJobState = stateData.Name;
                }
            }


            if (dataJob.TryGetValue("LastExecution", out var value3))
            {

                var tempLastExecution = JobHelper.DeserializeNullableDateTime(value3);

                dto.LastExecution = tempLastExecution.HasValue ? tempLastExecution.Value.ChangeTimeZone(dto.TimeZoneId).ToString("G") : "N/A";
            }

            if (dataJob.TryGetValue("CreatedAt", out var value4))
            {
                dto.CreatedAt = JobHelper.DeserializeNullableDateTime(value4);
                dto.CreatedAt = dto.CreatedAt?.ChangeTimeZone(dto.TimeZoneId) ?? new DateTime();
            }

            if (dataJob.TryGetValue("Error", out var error) && !string.IsNullOrEmpty(error))
            {
                dto.Error = error;
            }

            dto.Removed = false;
            dto.JobState = status;

            return dto;
        }

        public static bool IsValidJobId(string jobId, string tag = TagRecurringJob)
        {
            var result = false;
            using var connection = JobStorage.Current.GetConnection();
            var job = connection.GetAllEntriesFromHash($"{tag}:{jobId}");

            result = job != null;
            return result;
        }


    }
}
