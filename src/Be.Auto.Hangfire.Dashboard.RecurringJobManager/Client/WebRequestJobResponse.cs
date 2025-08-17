using System.Net;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Client;

public class WebRequestJobResponse
{
    public HttpStatusCode StatusCode { get; set; }

    public string ExceptionCode { get; set; }
    public string ExceptionMessage { get; set; }
}