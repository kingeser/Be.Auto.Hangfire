using System;
using System.Collections.Generic;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;
using Newtonsoft.Json.Linq;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Client;

public class WebRequestJobBodyJson
{
    public required Uri Uri { get; set; }
    public required HttpMethodType Method { get; set; }
    public List<HttpHeaderParameter> HeaderParameters { get; set; }
    public required JObject BodyParameters { get; set; }
    public BodyParameterType BodyParameterType => BodyParameterType.Json;
}