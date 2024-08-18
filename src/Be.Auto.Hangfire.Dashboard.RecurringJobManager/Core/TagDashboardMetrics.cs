using Hangfire.Dashboard;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core
{
    public static class TagDashboardMetrics
    {
        public static readonly DashboardMetric JobsStoppedCount = new DashboardMetric("JobsStopped:count", razorPage => new Metric(RecurringJobAgent.GetAllJobStopped().Count));
    }
}
