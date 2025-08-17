using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Formatting = Newtonsoft.Json.Formatting;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Client
{
    public class HangfireWebRequestJobApiClient(Uri baseUri) : IHangfireWebRequestJobApiClient
    {
        public List<HttpHeaderParameter> Headers { get; set; } = [];

        public async Task<WebRequestJobResponse> AddAsync(WebRequestJobBodyJson job)
        {

            return await AddAsync(new WebRequestJob()
            {
                UrlPath = job.Uri?.PathAndQuery,
                HostName = $"{job.Uri?.Scheme}://{job.Uri?.Host}{(job.Uri?.IsDefaultPort == true ? "" : $":{job.Uri?.Port}")}",
                BodyParameters = job.BodyParameters?.ToString(Formatting.None),
                BodyParameterType = job.BodyParameterType,
                HeaderParameters = job.HeaderParameters ?? [],
                HttpMethod = job.Method,

            });
        }

        public async Task<WebRequestJobResponse> AddAsync(WebRequestJobBodyXml job)
        {
            return await AddAsync(new WebRequestJob()
            {

                UrlPath = job.Uri?.PathAndQuery,
                HostName = $"{job.Uri?.Scheme}://{job.Uri?.Host}{(job.Uri?.IsDefaultPort == true ? "" : $":{job.Uri?.Port}")}",
                BodyParameters = job.BodyParameters?.OuterXml,
                BodyParameterType = job.BodyParameterType,
                HeaderParameters = job.HeaderParameters,
                HttpMethod = job.Method,

            });
        }

        public async Task<WebRequestJobResponse> AddAsync(WebRequestJobBodyFormUrlEncoded job)
        {
            return await AddAsync(new WebRequestJob()
            {

                UrlPath = job.Uri?.PathAndQuery,
                HostName = $"{job.Uri?.Scheme}://{job.Uri?.Host}{(job.Uri?.IsDefaultPort == true ? "" : $":{job.Uri?.Port}")}",
                BodyParameters = job.BodyParameters?.SerializeObjectToJson(),
                BodyParameterType = job.BodyParameterType,
                HeaderParameters = job.HeaderParameters,
                HttpMethod = job.Method,

            });
        }

        public async Task<WebRequestJobResponse> AddAsync(WebRequestJobBodyFormData job)
        {
            return await AddAsync(new WebRequestJob()
            {
                UrlPath = job.Uri?.PathAndQuery,
                HostName = $"{job.Uri?.Scheme}://{job.Uri?.Host}{(job.Uri?.IsDefaultPort == true ? "" : $":{job.Uri?.Port}")}",
                BodyParameters = job.BodyParameters?.SerializeObjectToJson(),
                BodyParameterType = job.BodyParameterType,
                HeaderParameters = job.HeaderParameters,
                HttpMethod = job.Method,

            });
        }

        public async Task<WebRequestJobResponse> AddAsync(WebRequestJobBodyPlainText job)
        {
            return await AddAsync(new WebRequestJob()
            {
                UrlPath = job.Uri?.PathAndQuery,
                HostName = $"{job.Uri?.Scheme}://{job.Uri?.Host}{(job.Uri?.IsDefaultPort == true ? "" : $":{job.Uri?.Port}")}",
                BodyParameters = job.BodyParameters,
                BodyParameterType = job.BodyParameterType,
                HeaderParameters = job.HeaderParameters,
                HttpMethod = job.Method,

            });
        }

        public async Task<WebRequestJobResponse> AddAsync(WebRequestJobBodyNone job)
        {
            return await AddAsync(new WebRequestJob()
            {

                UrlPath = job.Uri?.PathAndQuery,
                HostName = $"{job.Uri?.Scheme}://{job.Uri?.Host}{(job.Uri?.IsDefaultPort == true ? "" : $":{job.Uri?.Port}")}",
                BodyParameters = string.Empty,
                BodyParameterType = job.BodyParameterType,
                HeaderParameters = job.HeaderParameters,
                HttpMethod = job.Method,

            });
        }


        private async Task<WebRequestJobResponse> AddAsync(WebRequestJob job)
        {
            try
            {

                var uri = new Uri($"{baseUri.Scheme}://{baseUri.Host}{(baseUri.IsDefaultPort ? "" : $":{baseUri.Port}")}/api/web-request-job");

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);

                httpWebRequest.Timeout = Convert.ToInt32(Options.Instance.WebRequestJob?.TimeOut.TotalMilliseconds ??
                                                         TimeSpan.FromSeconds(30).TotalMilliseconds);

                httpWebRequest.Method = "POST";

                httpWebRequest.ContentType = "application/json";

                // Header ekleme
                if (Headers != null)
                {
                    foreach (var header in Headers)
                    {
                        httpWebRequest.Headers[header.Name] = header.Value;
                    }
                }

                var jsonBody = job.SerializeObjectToJson();
                using (var requestStream = await httpWebRequest.GetRequestStreamAsync())
                using (var writer = new StreamWriter(requestStream, Encoding.UTF8))
                {
                    await writer.WriteAsync(jsonBody);
                    await writer.FlushAsync();
                }

                // Response okuma
                using var response = (HttpWebResponse)await httpWebRequest.GetResponseAsync();



                return new WebRequestJobResponse()
                {
                    ExceptionCode = string.Empty,
                    ExceptionMessage = string.Empty,
                    StatusCode = response.StatusCode
                };
            }
            catch (Exception ex)
            {
                if (ex is not WebException { Response: HttpWebResponse errorResponse })
                    return new WebRequestJobResponse()
                    {
                        ExceptionCode = ex.GetType().Name,
                        ExceptionMessage = ex.Message,
                        StatusCode = HttpStatusCode.BadRequest
                    };

                using var reader = new StreamReader(errorResponse.GetResponseStream() ?? Stream.Null);

                var errorContent = $"{(await reader.ReadToEndAsync())}";

                return new WebRequestJobResponse()
                {
                    ExceptionCode = ex.GetType().Name,
                    ExceptionMessage = errorContent,
                    StatusCode = HttpStatusCode.BadRequest
                };

            }

        }
    }
}
