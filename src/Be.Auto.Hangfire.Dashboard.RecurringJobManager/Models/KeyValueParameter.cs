using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;



public class HttpHeaderParameter
{
    public string Name { get; set; }
    public string Value { get; set; }
   
}

public class HttpFormUrlEncodedParameter
{
    public string Name { get; set; }
    public string Value { get; set; }

}

public class HttpFormDataParameter
{
    public string Name { get; set; }
    public string Value { get; set; }
    public string ContentType { get; set; }

}