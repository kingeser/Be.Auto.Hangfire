using System;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;
using Hangfire;
using Hangfire.States;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Attributes
{
    /// <summary>
	/// Attribute to add or update <see cref="RecurringJob"/> automatically
	/// by target it to interface/instance/static method.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
    public class RecurringJobAttribute : Attribute
    {
        public JobType JobType => JobType.MethodCall;

        public MisfireHandlingMode MisfireHandlingMode { get; set; }

        /// <summary>
        /// The identifier of the RecurringJob
        /// </summary>
        public string RecurringJobId { get; set; }
        /// <summary>
        /// Cron expressions
        /// </summary>
        public string Cron { get; set; }
        /// <summary>
        /// Queue name
        /// </summary>
        public string Queue { get; set; }
        /// <summary>
        /// Converts to <see cref="TimeZoneInfo"/> via method <seealso cref="TimeZoneInfo.FindSystemTimeZoneById(string)"/>,
        /// default value is <see cref="TimeZoneInfo.Utc"/>
        /// </summary>
        public TimeZoneInfo TimeZone { get; set; }
        /// <summary>
        /// Whether to build RecurringJob automatically, default value is true.
        /// If false it will be deleted automatically.
        /// </summary>
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringJobAttribute"/>
        /// </summary>
        /// <param name="cron">Cron expressions</param>
        public RecurringJobAttribute(string cron) : this(cron, EnqueuedState.DefaultQueue) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringJobAttribute"/>
        /// </summary>
        /// <param name="cron">Cron expressions</param>
        /// <param name="queue">Queue name</param>
        public RecurringJobAttribute(string cron, string queue) : this(cron, TimeZoneInfo.Utc, queue) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringJobAttribute"/>
        /// </summary>
        /// <param name="cron">Cron expressions</param>
        /// <param name="timeZone">Converts to <see cref="TimeZoneInfo"/> via method <seealso cref="TimeZoneInfo.FindSystemTimeZoneById(string)"/>.</param>
        /// <param name="queue">Queue name</param>
        public RecurringJobAttribute(string cron, TimeZoneInfo timeZone, string queue)
        {
            if (string.IsNullOrEmpty(cron)) throw new ArgumentNullException(nameof(cron));
            if (string.IsNullOrEmpty(queue)) throw new ArgumentNullException(nameof(queue));

            Cron = cron;
            TimeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
            Queue = queue;
        }
    }
}
