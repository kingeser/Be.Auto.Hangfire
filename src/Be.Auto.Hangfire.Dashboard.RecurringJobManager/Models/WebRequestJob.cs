using System.Collections.Generic;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;

public class WebRequestJob
{
    public string HostName { get; set; }
    public string UrlPath { get; set; }
    public HttpMethodType HttpMethod { get; set; }
    public List<HttpHeaderParameter> HeaderParameters { get; set; }
    public BodyParameterType BodyParameterType { get; set; }
    public string BodyParameters { get; set; }
}