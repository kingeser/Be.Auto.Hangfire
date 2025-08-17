using System;
using System.Collections.Generic;
using System.Linq;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;
using Hangfire;
using Hangfire.Common;
using Hangfire.Dashboard;
using Hangfire.Storage;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;
public static class DisplayNameFunctions
{
    private static DateTime? _latestRefreshTime;

    private static List<Tuple<string, InvocationData>> _jobInvocationDatas = new();

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(1);

    private static List<Tuple<string, InvocationData>> JobInvocationDatas
    {
        get
        {
            if (_latestRefreshTime != null && (DateTime.Now - _latestRefreshTime.Value) < CacheDuration)
                return _jobInvocationDatas;

            _jobInvocationDatas = GetJobInvocationDatas();

            _latestRefreshTime = DateTime.Now;

            return _jobInvocationDatas;
        }
    }

    private static List<Tuple<string, InvocationData>> GetJobInvocationDatas()
    {
        var jobInvocationDatas = new List<Tuple<string, InvocationData>>();

        using var connection = JobStorage.Current.GetConnection();

        var jobIds = connection.GetAllItemsFromSet("recurring-jobs");

        foreach (var jobId in jobIds)
        {
            var entries = connection.GetAllEntriesFromHash($"recurring-job:{jobId}");

            if (!entries.TryGetValue("Job", out var jobValue)) continue;

            var invocationData = InvocationData.DeserializePayload(jobValue);

            jobInvocationDatas.Add(new Tuple<string, InvocationData>(jobId, invocationData));
        }

        return jobInvocationDatas;
    }

    private static Tuple<string, InvocationData>? FindMatchingInvocationData(InvocationData jobInvocationData)
    {
        return JobInvocationDatas.FirstOrDefault(entry =>
            entry.Item2.Type.CleanVersionDetails() == jobInvocationData.Type.CleanVersionDetails() &&
            entry.Item2.Method == jobInvocationData.Method &&
            entry.Item2.Arguments == jobInvocationData.Arguments &&
            entry.Item2.ParameterTypes.CleanVersionDetails() == jobInvocationData.ParameterTypes.CleanVersionDetails());
    }
    public static string WithJobId(DashboardContext context, Job job)
    {
        var jobInvocationData = InvocationData.SerializeJob(job);

        var existJobInvocationData = FindMatchingInvocationData(jobInvocationData);

        return existJobInvocationData != null ? $"{existJobInvocationData.Item1} » {job.Type.Name}.{job.Method.Name}({string.Join(",", job.Method.GetParameters().Select(t => $"{t.ParameterType.Name} {t.Name}"))})" : $"{job.Type.Name}.{job.Method.Name}({string.Join(",", job.Method.GetParameters().Select(t => $"{t.ParameterType.Name} {t.Name}"))})";
    }
}