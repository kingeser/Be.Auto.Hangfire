using System;
using System.Collections.Generic;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Client;

public class WebRequestJobBodyPlainText
{
    public required Uri Uri { get; set; }
    public required HttpMethodType Method { get; set; }
    public List<HttpHeaderParameter> HeaderParameters { get; set; }
    public required string BodyParameters { get; set; }
    public BodyParameterType BodyParameterType => BodyParameterType.PlainText;
}