using System.Threading.Tasks;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Client;

public interface IHangfireWebRequestJobApiClient
{
    Task<WebRequestJobResponse> AddAsync(WebRequestJobBodyJson job);
    Task<WebRequestJobResponse> AddAsync(WebRequestJobBodyXml job);
    Task<WebRequestJobResponse> AddAsync(WebRequestJobBodyFormUrlEncoded job);
    Task<WebRequestJobResponse> AddAsync(WebRequestJobBodyFormData job);
    Task<WebRequestJobResponse> AddAsync(WebRequestJobBodyPlainText job);
    Task<WebRequestJobResponse> AddAsync(WebRequestJobBodyNone job);
}