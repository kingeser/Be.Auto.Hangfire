using System;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;
using Hangfire.Common;
using Hangfire.States;
using System.Collections.Generic;
using System.Linq;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Attributes
{
    internal class DisableConcurrentlyJobExecutionAttribute : JobFilterAttribute, IElectStateFilter
    {

        public void OnStateElection(ElectStateContext context)
        {
            if (context.CandidateState is FailedState) return;
            if (context.CandidateState is not ProcessingState) return;
            if (context.BackgroundJob.Job == null) return;
            var processingJobs = context.Storage.GetMonitoringApi().ProcessingJobs(0, int.MaxValue);
            if (processingJobs.Count <= 0) return;

            var jobData = RecurringJobAgent.GetJob(context.BackgroundJob.Job);

            if (jobData is { LimitConcurrency: false }) return;

            var type = context.BackgroundJob.Job.Type.FullName;
            var methodName = context.BackgroundJob.Job.Method.GenerateFullName();
            var recurringJobId = context.BackgroundJob.Job.ToString();
            var args = context.BackgroundJob.Job.Args;
            var inProgressCount = processingJobs.Count(t =>
                t.Value.Job.Method.GenerateFullName().Equals(methodName, StringComparison.InvariantCultureIgnoreCase)
                && $"{t.Value.Job.Type.FullName}".Equals(type, StringComparison.InvariantCultureIgnoreCase)
                && t.Value.Job.ToString() == recurringJobId
                && AreArgsEqual(t.Value.Job.Args, args)
                && !context.CandidateState.IsFinal);

            var maxConcurrentTasks = jobData?.MaxConcurrentTasks ?? 1;

            if (inProgressCount >= maxConcurrentTasks)
            {


                context.CandidateState = new CancelledState(context.BackgroundJob.CreatedAt, maxConcurrentTasks);
            }

        }

        public bool AreArgsEqual(IReadOnlyList<object> args1, IReadOnlyList<object> args2)
        {

            var arg1Json = args1.SerializeObjectToJson();
            var arg2Json = args2.SerializeObjectToJson();

            return string.Equals(arg1Json, arg2Json);

        }

    }
}
