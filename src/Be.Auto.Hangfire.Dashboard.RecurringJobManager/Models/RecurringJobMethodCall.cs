using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;

public class RecurringJobMethodCall : RecurringJobBase
{
    public override JobType JobType => JobType.MethodCall;
    public string Type { get; set; }
    public string Method { get; set; }
    public string MethodParameters { get; set; }

}