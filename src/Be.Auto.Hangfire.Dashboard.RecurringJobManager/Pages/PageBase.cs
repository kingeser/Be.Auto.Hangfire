using Hangfire.Dashboard;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Pages
{
    public abstract  class PageBase : RazorPage
    {
        public override void Execute() { }

        protected void WriteLiteralLine(string textToAppend)
        {
            WriteLiteral(textToAppend);
            WriteLiteral("\r\n");
        }

        protected void WriteEmptyLine()
        {
            WriteLiteral("\r\n");
        }
    }
}
