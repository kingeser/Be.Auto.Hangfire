using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;
using Hangfire.Dashboard.Pages;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Pages;

public sealed class JobsCancelledPage : PageBase
{
    public const string Title = "Cancelled Jobs";
    public const string PageRoute = "/jobs/cancelled";

    private static readonly string PageHtml;

    static JobsCancelledPage()
    {
        PageHtml = ManifestResource.ReadStringResource("Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.CancelledJobs.html");
    }

    public override void Execute()
    {
        WriteEmptyLine();
        Layout = new LayoutPage(Title);
        Write(Html.JobsSidebar());
        WriteLiteralLine(PageHtml);
        WriteEmptyLine();
    }
}