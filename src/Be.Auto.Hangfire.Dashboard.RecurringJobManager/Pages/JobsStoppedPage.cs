using Hangfire.Dashboard.Pages;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Pages
{
    internal sealed class JobsStoppedPage : PageBase
    {
        public const string Title = "Stopped Jobs";
        public const string PageRoute = "/jobs/stopped";

        private static readonly string PageHtml;

        static JobsStoppedPage()
        {
            PageHtml = ManifestResource.ReadStringResource("Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.JobsStopped.html");
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
}
