using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hangfire.Sample.Pages
{
    public class IndexModel : PageModel
    {
     
        public void OnGet()
        {
            Redirect("/hangfire");
        }
    }
}
