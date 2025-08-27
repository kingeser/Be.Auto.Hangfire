using Hangfire.Dashboard;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core
{
    internal static class TagDashboardMetrics
    {
        public static readonly DashboardMetric JobsCountMetric = new DashboardMetric("Jobs:count", razorPage => new Metric(RecurringJobAgent.GetAllJobs().Count));
        public static readonly DashboardMetric JobsStoppedCount = new DashboardMetric("JobsStopped:count", razorPage => new Metric(RecurringJobAgent.GetAllJobStopped().Count));
        public static readonly DashboardMetric JobsCancelledCount = new DashboardMetric("JobsCancelled:count", razorPage => new Metric(RecurringJobAgent.GetCancelledJobs().Count));

       
    }


}
