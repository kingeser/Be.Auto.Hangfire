﻿using System;
using System.Linq;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;
using Hangfire.Common;
using Hangfire.States;
using System.Collections.Generic;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Attributes
{
    internal class DisableConcurrentlyJobExecutionAttribute : JobFilterAttribute, IElectStateFilter
    {
        
        public void OnStateElection(ElectStateContext context)
        {
            if (context.BackgroundJob.Job == null) return;

            var processingJobs = context.Storage.GetMonitoringApi().ProcessingJobs(0, int.MaxValue);

            if (processingJobs.Count <= 0) return;

            var type = context.BackgroundJob.Job.Type.FullName;
            var methodName = context.BackgroundJob.Job.Method.GenerateFullName();
            var recurringJobId = context.BackgroundJob.Job.ToString();
            var args = context.BackgroundJob.Job.Args;

            if (processingJobs.Exists(t =>
                    t.Value.Job.Method.GenerateFullName().Equals(methodName, StringComparison.InvariantCultureIgnoreCase)
                    && $"{t.Value.Job.Type.FullName}".Equals(type, StringComparison.InvariantCultureIgnoreCase)
                    && t.Value.Job.ToString() == recurringJobId
                    && AreArgsEqual(t.Value.Job.Args, args)
                    && !context.CandidateState.IsFinal))
            {
             
                context.CandidateState = new CancelledState(context.BackgroundJob.CreatedAt);
            }
        }

        public bool AreArgsEqual(IReadOnlyList<object> args1, IReadOnlyList<object> args2)
        {
            if (args1 == null || args2 == null)
            {
                return true;
            }

            if (ReferenceEquals(args1, args2))
            {
                return true;
            }

            return args1.Count == args2.Count && args1.SequenceEqual(args2);
        }

    }
}
