using Hangfire.Sample.Library;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hangfire.JobExtensions.Pages
{
    public class IndexModel : PageModel
    {
     
        public void OnGet()
        {
            Redirect("/hangfire");
        }
    }
}
