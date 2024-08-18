using System;
using System.Collections.Generic;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;
using Hangfire;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models
{

    public class PeriodicJob
    {
        public string Id { get; set; }
        public string Cron { get; set; }
        public JobType JobType { get; set; }
        public string Class { get; set; }
        public string Method { get; set; }
        public object[] MethodParameters { get; set; }
        public HttpMethodType HttpMethod { get; set; }
        public BodyParameterType BodyParameterType { get; set; }
        public MisfireHandlingMode MisfireHandlingMode { get; set; }
        public List<BodyParameter> BodyParameters { get; set; }
        public List<HeaderParameter> HeaderParameters { get; set; }
        public string JobState { get; set; }
        public string NextExecution { get; set; }
        public string LastJobId { get; set; }
        public string LastJobState { get; set; }
        public string LastExecution { get; set; }
        public DateTime? CreatedAt { get; set; }
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


    public class BodyParameter
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
    public class HeaderParameter
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}

