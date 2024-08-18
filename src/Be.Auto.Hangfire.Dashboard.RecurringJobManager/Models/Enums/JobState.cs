namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums
{
    /// <summary>
    /// Represents the possible states of a background job in Hangfire.
    /// </summary>
    public enum JobState
    {
        /// <summary>
        /// The job has been marked as deleted and will not be executed.
        /// </summary>
        DeletedState,

        /// <summary>
        /// The job has failed during execution.
        /// </summary>
        FailedState,

        /// <summary>
        /// The job has been queued and is waiting to be processed.
        /// </summary>
        EnqueuedState,
    }

}
