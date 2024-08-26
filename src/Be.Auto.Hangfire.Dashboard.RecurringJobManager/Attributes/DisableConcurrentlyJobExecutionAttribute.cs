using System;
using System.Linq;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;
using Hangfire.Common;
using Hangfire.States;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;
using System.Collections.Generic;
using Hangfire.Dashboard;
using Hangfire;
using Hangfire.Server;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Attributes
{

    public class DisableConcurrentlyJobExecutionAttribute : JobFilterAttribute, IElectStateFilter
    {

        private const string DefaultReason = "It is not allowed to perform multiple same tasks.";

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
                    && t.Value.Job.Type.FullName.Equals(type, StringComparison.InvariantCultureIgnoreCase)
                    && t.Value.Job.ToString() == recurringJobId
                    && AreArgsEqual(t.Value.Job.Args, args)
                    && !context.CandidateState.IsFinal))
            {
                context.CandidateState = new DeletedState()
                {
                    Reason = DefaultReason,

                };
            }
        }

        public bool AreArgsEqual(IReadOnlyList<object> args1, IReadOnlyList<object> args2)
        {

            if (ReferenceEquals(args1, args2))
            {
                return true;
            }


            if (args1 == null || args2 == null)
            {
                return true;
            }


            if (args1.Count != args2.Count)
            {
                return false;
            }

            return args1.SequenceEqual(args2);
        }

    }
}
