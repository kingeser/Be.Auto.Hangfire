using System.Collections.Generic;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;

public class RecurringJobWebRequest : RecurringJobBase
{
    public override JobType JobType => JobType.WebRequest;
    public string HostName { get; set; }
    public string UrlPath { get; set; }
    public HttpMethodType HttpMethod { get; set; }
    public BodyParameterType BodyParameterType { get; set; }
    public List<BodyParameter> BodyParameters { get; set; }
    public List<HeaderParameter> HeaderParameters { get; set; }

}