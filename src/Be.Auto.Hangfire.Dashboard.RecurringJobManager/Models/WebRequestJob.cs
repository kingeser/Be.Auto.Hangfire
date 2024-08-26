using System.Collections.Generic;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;

internal class WebRequestJob
{
    public required string HostName { get; set; }
    public required string UrlPath { get; set; }
    public required HttpMethodType HttpMethod { get; set; }
    public required List<HttpHeaderParameter> HeaderParameters { get; set; }
    public required BodyParameterType BodyParameterType { get; set; }
    public required string BodyParameters { get; set; }
}

