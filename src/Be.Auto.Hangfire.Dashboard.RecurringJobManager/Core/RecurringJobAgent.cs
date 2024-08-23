﻿using Hangfire.Common;
using Hangfire.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;
using Hangfire;
using Newtonsoft.Json;


namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core
{
    public static class RecurringJobAgent
    {
        public const string TagRecurringJobBase = "recurring-job";
        public const string TagStopJob = "recurring-jobs-stop";
        public static void StartBackgroundJob(params string[] jobId)
        {
            using var connection = JobStorage.Current.GetConnection();
            using var transaction = connection.CreateWriteTransaction();
            foreach (var id in jobId)
            {
                transaction.RemoveFromSet(TagStopJob, id);
                transaction.AddToSet($"{TagRecurringJobBase}s", id);
            }
            transaction.Commit();
        }
        public static void StopBackgroundJob(params string[] jobId)
        {
            using var connection = JobStorage.Current.GetConnection();
            using var transaction = connection.CreateWriteTransaction();
            foreach (var id in jobId)
            {
                transaction.RemoveFromSet($"{TagRecurringJobBase}s", id);
                transaction.AddToSet($"{TagStopJob}", id);
            }

            transaction.Commit();
        }

        public static void DeleteJobDetails(string jobId)
        {
            using var connection = JobStorage.Current.GetConnection();
            using var transaction = connection.CreateWriteTransaction();
            transaction.RemoveHash($"{TagRecurringJobBase}:{jobId}");
            transaction.Commit();
        }
        public static void SaveJobDetails(RecurringJobBase job)
        {
            using var connection = JobStorage.Current.GetConnection();
            using var transaction = connection.CreateWriteTransaction();

            var details = new List<KeyValuePair<string, string>>
            {
                new(nameof(job.JobType), job.JobType.ToString()),
                new(nameof(job.MisfireHandlingMode), job.MisfireHandlingMode.ToString()),
                new(nameof(job.Guid), job.Guid),
            };

            switch (job.JobType)
            {
                case JobType.MethodCall:
                    if (job is RecurringJobMethodCall methodCallJob)
                    {
                        details.Add(new(nameof(methodCallJob.MethodParameters), JsonConvert.SerializeObject(methodCallJob.MethodParameters)));

                    }

                    break;
                case JobType.WebRequest:
                    if (job is RecurringJobWebRequest webRequestJob)
                    {
                        details.Add(new(nameof(webRequestJob.UrlPath), webRequestJob.UrlPath));
                        details.Add(new(nameof(webRequestJob.HostName), webRequestJob.HostName));
                        details.Add(new(nameof(webRequestJob.HttpMethod), webRequestJob.HttpMethod.ToString()));
                        details.Add(new(nameof(webRequestJob.BodyParameterType), webRequestJob.BodyParameterType.ToString()));
                        details.Add(new(nameof(webRequestJob.BodyParameters), JsonConvert.SerializeObject(webRequestJob.BodyParameters)));
                        details.Add(new(nameof(webRequestJob.HeaderParameters), JsonConvert.SerializeObject(webRequestJob.HeaderParameters)));

                    }
                    break;
            }

            transaction.SetRangeInHash($"{TagRecurringJobBase}:{job.Id}", details);

            transaction.Commit();
        }

        public static List<RecurringJobBase> GetAllJobStopped() => GetAllJobs().Where(t => t.JobState == "Stopped").ToList();

        public static RecurringJobBase GetJob(string jobId) => GetAllJobs().Find(t => t.Id == jobId);
        public static List<RecurringJobBase> GetAllJobs()
        {
            var result = new List<RecurringJobBase>();

            using var connection = JobStorage.Current.GetConnection();

            var runningJobs = connection.GetRecurringJobs();

            runningJobs.ForEach(recurringJobBaseDto =>
            {
                var dto = MapPeriodicJob(connection, recurringJobBaseDto.Id, "Running", recurringJobBaseDto.Removed);
                if (dto == null) return;
                result.Add(dto);
            });

            var allJobStopped = connection.GetAllItemsFromSet(TagStopJob).ToList();

            allJobStopped.ForEach(jobId =>
            {
                var dto = MapPeriodicJob(connection, jobId, "Stopped", false);
                if (dto == null) return;
                result.Add(dto);
            });

            return (from a in result
                    group a by a.Guid
                into g
                    select g.FirstOrDefault()).ToList();
        }

        private static RecurringJobBase MapPeriodicJob(IStorageConnection connection, string jobId, string status, bool removed)
        {
            var dataJob = connection.GetAllEntriesFromHash($"{TagRecurringJobBase}:{jobId}");

            if (dataJob == null) return default;

            var jobType = JobType.MethodCall;
            if (dataJob.TryGetValue(nameof(RecurringJobBase.JobType), out var jobTypeValue) && !string.IsNullOrWhiteSpace(jobTypeValue))
            {
                jobType = (JobType)Enum.Parse(typeof(JobType), jobTypeValue);
            }


            RecurringJobBase dto = null;


            switch (jobType)
            {
                case JobType.MethodCall:
                    dto = dataJob.BindFromDictionary<RecurringJobMethodCall>();
                    break;
                case JobType.WebRequest:
                    dto = dataJob.BindFromDictionary<RecurringJobWebRequest>();
                    break;

            }

            if (dto == null) return default;


            dto.Id = jobId;

            dto.TimeZoneId = "UTC";

            try
            {
                if (dataJob.TryGetValue("Job", out var payload) && !string.IsNullOrWhiteSpace(payload))
                {
                    if (dto is RecurringJobMethodCall methodCallJob)
                    {
                        var invocationData = InvocationData.DeserializePayload(payload);
                        var job = invocationData.DeserializeJob();
                        methodCallJob.Method = job.Method.GenerateFullName();
                        methodCallJob.Type = job.Type.FullName;
                    }


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

            dto.Removed = removed;
            dto.JobState = status;

            return dto;
        }

        public static bool IsValidJobId(string jobId, string tag = TagRecurringJobBase)
        {
            var result = false;
            using var connection = JobStorage.Current.GetConnection();
            var job = connection.GetAllEntriesFromHash($"{tag}:{jobId}");

            result = job != null;
            return result;
        }

        public static bool IsJobIdExist(RecurringJobBase job)
        {
            using var connection = JobStorage.Current.GetConnection();

            var entries = connection.GetAllEntriesFromHash($"{TagRecurringJobBase}:{job.Id}");

            if (entries == null)
                return false;

            entries.TryGetValue(nameof(RecurringJobBase.Guid), out var guidValue);

            if (string.IsNullOrEmpty(guidValue)) return false;


            if (job.Guid == guidValue)
            {
                return false;
            }


            return true;
        }
    }
}
