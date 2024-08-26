using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;

internal class CancelledJob
{
    public string Id { get; set; }
    public string Reason { get; set; }
    public string Job { get; set; }
    public string CancelledAt { get; set; }
    public JobType Type { get; set; }
}