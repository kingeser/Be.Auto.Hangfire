using System;
using System.Linq;
using Hangfire.Common;
using Hangfire.States;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Attributes
{
    //Resharper disable all
    public class DisableConcurrentlyJobExecutionAttribute : JobFilterAttribute, IElectStateFilter
    {
        private const int DefaultFrom = 0;
        private const int DefaultCount = 2000;
        private const string DefaultReason = "It is not allowed to perform multiple same tasks.";
        private const JobState DefaultJobState = JobState.DeletedState;

        private string _methodName;
        private string _reason;

        public DisableConcurrentlyJobExecutionAttribute()
        {
            From = DefaultFrom;
            Count = DefaultCount;
            JobState = DefaultJobState;
            _reason = DefaultReason;
        }

        public DisableConcurrentlyJobExecutionAttribute(string methodName)
            : this()
        {
            MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
        }

        public DisableConcurrentlyJobExecutionAttribute(string methodName, JobState jobState)
            : this(methodName)
        {
            JobState = jobState;
        }

        public DisableConcurrentlyJobExecutionAttribute(string methodName, int from, int count, JobState jobState = DefaultJobState)
            : this(methodName, jobState)
        {
            From = from;
            Count = count;
        }

        public DisableConcurrentlyJobExecutionAttribute(string methodName, int from, int count, string reason, JobState jobState = DefaultJobState)
            : this(methodName, from, count, jobState)
        {
            Reason = reason ?? DefaultReason;
        }

        public int From { get; } = DefaultFrom;
        public int Count { get; } = DefaultCount;
        public JobState JobState { get; }
        public string MethodName
        {
            get => _methodName;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(nameof(MethodName));
                _methodName = value;
            }
        }

        public string Reason
        {
            get => _reason;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentNullException(nameof(Reason));
                _reason = value;
            }
        }

        public void OnStateElection(ElectStateContext context)
        {
            MethodName ??= context.BackgroundJob.Job.Method.Name;

            var processingJobs = context.Storage.GetMonitoringApi().ProcessingJobs(From, Count);

            if (processingJobs.Any(processingJob => processingJob.Value.Job.Method.Name.Equals(MethodName, StringComparison.InvariantCultureIgnoreCase) && !context.CandidateState.IsFinal))
            {
                context.CandidateState = context.CandidateState switch
                {
                    FailedState failedState => new FailedState(failedState.Exception) { Reason = Reason },
                    _ => CreateNewState()
                };
            }
        }

        private IState CreateNewState()
        {
            return JobState switch
            {
                JobState.DeletedState => new DeletedState { Reason = Reason },
                JobState.FailedState => new FailedState(new Exception(Reason)) { Reason = Reason },
                JobState.EnqueuedState => new EnqueuedState { Reason = Reason },
                _ => throw new InvalidOperationException($"Unsupported job state: {JobState}")
            };
        }
    }
}
