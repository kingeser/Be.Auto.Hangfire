using Hangfire.Dashboard.Pages;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Pages
{
    internal sealed class JobExtensionPage : PageBase
    {
        public const string Title = "Recurring Job Manager";
        public const string PageRoute = "/job-manager";

        private static readonly string PageHtml;

        static JobExtensionPage()
        {
            PageHtml = ManifestResource.ReadStringResource("Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.JobManager.html");
        }

        public override void Execute()
        {
            WriteEmptyLine();
            Layout = new LayoutPage(Title);
            WriteLiteralLine(PageHtml);
            WriteEmptyLine();
        }


    }
}
