using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Client;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;
using Newtonsoft.Json.Linq;

namespace Sample.Test;

public class HangfireApiClientTest(IHangfireWebRequestJobApiClient apiClient)
{

    public async Task TestAsync()
    {

        await apiClient.AddAsync(new WebRequestJobBodyJson()
        {
            BodyParameters = JObject.Parse("{}"),
            Method = HttpMethodType.POST,
            Uri = new Uri("htt543ps://www.google.543com/search=543burak eser"),
            HeaderParameters =new List<HttpHeaderParameter>(),
            
        });

    }

}