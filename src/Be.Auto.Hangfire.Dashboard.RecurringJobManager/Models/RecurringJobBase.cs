using System;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;
using Hangfire;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models
{
    internal abstract class RecurringJobBase
    {
        public  string Guid { get; set; }
        public string Id { get; set; }
        public string Cron { get; set; }
        public abstract JobType JobType { get; }
        public MisfireHandlingMode MisfireHandlingMode { get; set; }
        public string JobState { get; set; }
        public string NextExecution { get; set; }
        public string LastJobId { get; set; }
        public string LastJobState { get; set; }
        public string LastExecution { get; set; }
        public string CreatedAt { get; set; }
        public bool Removed { get; set; }
        public string TimeZoneId { get; set; }

        public TimeZoneInfo TimeZone
        {
            get
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
                }
                catch
                {
                    return TimeZoneInfo.Utc;
                }
            }
        }

        public string Error { get; set; }
    }
}

