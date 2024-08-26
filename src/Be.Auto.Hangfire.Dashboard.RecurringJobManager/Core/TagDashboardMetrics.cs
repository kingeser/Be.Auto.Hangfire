using System;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Storage.Monitoring;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core
{
    public static class TagDashboardMetrics
    {
        public static readonly DashboardMetric JobsStoppedCount = new DashboardMetric("JobsStopped:count", razorPage => new Metric(RecurringJobAgent.GetAllJobStopped().Count));
        public static readonly DashboardMetric JobsCancelledCount = new DashboardMetric("JobsCancelled:count", razorPage => new Metric(RecurringJobAgent.GetCancelledJobs().Count));

       
    }


}
