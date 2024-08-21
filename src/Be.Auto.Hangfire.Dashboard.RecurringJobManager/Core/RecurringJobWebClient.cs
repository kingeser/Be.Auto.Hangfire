using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;
using System;
using System.Linq;
using System.Threading.Tasks;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions
{
    public class RecurringJobWebClient
    {
        public async Task<object> CallRequestAsync(WebRequestJob job)
        {

            var baseUri = new Uri(job.HostName.TrimEnd('/'));

            var url = new Uri(baseUri, job.UrlPath.TrimStart('/')).ToString();

            if (IsSimpleRequest(job.HttpMethod))
            {
                var simpleResponse= await HandleSimpleRequestAsync(job, url);

                return new
                {
                    Response = simpleResponse,
                    ResponseString = await ReadResponseStreamAsync(simpleResponse)
                };
            }

            var response = IsComplexRequest(job.HttpMethod) ? await HandleComplexRequestAsync(job, url) : null;

            if (response == null) return null;

            return new
            {
                Response = response,
                ResponseString = await ReadResponseStreamAsync(response)
            };

        }

        private static async Task<string> ReadResponseStreamAsync(HttpWebResponse response)
        {
            using var responseStream = new StreamReader(response.GetResponseStream() ?? Stream.Null);

            var responseString= await responseStream.ReadToEndAsync();
            return responseString;
        }

        private static bool IsSimpleRequest(HttpMethodType method) => method is HttpMethodType.GET or HttpMethodType.DELETE or HttpMethodType.HEAD or HttpMethodType.OPTIONS or HttpMethodType.TRACE;

        private static async Task<HttpWebResponse> HandleSimpleRequestAsync(WebRequestJob job, string url)
        {
            var queryString = BuildQueryStringParameters(job);

            if (!url.EndsWith("?"))
                url += "?";

            url += queryString;

            var httpWebRequest = CreateWebRequest(job, url);

            return await GetResponseAsync(httpWebRequest);
        }

        private static bool IsComplexRequest(HttpMethodType method) => method is HttpMethodType.POST or HttpMethodType.PUT or HttpMethodType.PATCH;

        private static async Task<HttpWebResponse> HandleComplexRequestAsync(WebRequestJob job, string url)
        {
            var httpWebRequest = CreateWebRequest(job, url);

            return job.BodyParameterType switch
            {
                BodyParameterType.None => await SendRequestWithEmptyBodyAsync(httpWebRequest),
                BodyParameterType.Json => await SendRequestWithBodyAsync(httpWebRequest, job.BodyParameters, "application/json"),
                BodyParameterType.Xml => await SendRequestWithBodyAsync(httpWebRequest, job.BodyParameters, "application/xml"),
                BodyParameterType.FormUrlEncoded => await SendFormUrlEncodedRequestAsync(httpWebRequest, job.BodyParameters),
                BodyParameterType.FormData => await SendMultipartFormDataRequestAsync(httpWebRequest, job.BodyParameters),
                BodyParameterType.PlainText => await SendRequestWithBodyAsync(httpWebRequest, job.BodyParameters, "text/plain"),
                _ => null
            };
        }

        private static async Task<HttpWebResponse> SendRequestWithEmptyBodyAsync(HttpWebRequest httpWebRequest)
        {
            httpWebRequest.ContentLength = 0;
            return await GetResponseAsync(httpWebRequest);
        }

        private static async Task<HttpWebResponse> SendRequestWithBodyAsync(HttpWebRequest httpWebRequest, string body, string contentType)
        {
            httpWebRequest.ContentType = contentType;
            using (var requestStream = new StreamWriter(await httpWebRequest.GetRequestStreamAsync()))
            {
                if (!string.IsNullOrEmpty(body))
                {
                    await requestStream.WriteAsync(body);
                }
            }
            return await GetResponseAsync(httpWebRequest);
        }

        private static async Task<HttpWebResponse> SendFormUrlEncodedRequestAsync(HttpWebRequest httpWebRequest, string bodyParameters)
        {
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            var parameters = bodyParameters.DeserializeObjectFromJson<List<HttpFormUrlEncodedParameter>>();
            var formData = new StringBuilder();

            foreach (var parameter in parameters)
            {
                if (formData.Length > 0)
                {
                    formData.Append("&");
                }
                formData.Append($"{WebUtility.UrlEncode(parameter.Name)}={WebUtility.UrlEncode(parameter.Value)}");
            }

            using (var requestStream = new StreamWriter(await httpWebRequest.GetRequestStreamAsync()))
            {
                await requestStream.WriteAsync(formData.ToString());
            }

            return await GetResponseAsync(httpWebRequest);
        }

        private static async Task<HttpWebResponse> SendMultipartFormDataRequestAsync(HttpWebRequest httpWebRequest, string bodyParameters)
        {
            var boundary = "------------------------" + DateTime.Now.Ticks.ToString("x");
            httpWebRequest.ContentType = "multipart/form-data; boundary=" + boundary;

            var parameters = bodyParameters.DeserializeObjectFromJson<List<HttpFormDataParameter>>();

            using var requestStream = await httpWebRequest.GetRequestStreamAsync();

            foreach (var param in parameters)
            {
                var header = $"--{boundary}\r\nContent-Disposition: form-data; name=\"{param.Name}\"\r\n" +
                             $"Content-Type: {param.ContentType}\r\n\r\n";
                var headerBytes = Encoding.UTF8.GetBytes(header);

                await requestStream.WriteAsync(headerBytes, 0, headerBytes.Length);

                if (!string.IsNullOrEmpty(param.Value))
                {
                    var dataBytes = IsBase64String(param.Value)
                        ? Convert.FromBase64String(param.Value)
                        : Encoding.UTF8.GetBytes(param.Value);
                    await requestStream.WriteAsync(dataBytes, 0, dataBytes.Length);
                }

                var newlineBytes = "\r\n"u8.ToArray();
                await requestStream.WriteAsync(newlineBytes, 0, newlineBytes.Length);
            }

            return await GetResponseAsync(httpWebRequest);
        }

        private static async Task<HttpWebResponse> GetResponseAsync(HttpWebRequest httpWebRequest)
        {
            return (HttpWebResponse)await httpWebRequest.GetResponseAsync();
        }

        private static HttpWebRequest CreateWebRequest(WebRequestJob job, string url)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = job.HttpMethod.ToString();

            if (job.HeaderParameters == null || !job.HeaderParameters.Any()) return httpWebRequest;
            foreach (var header in job.HeaderParameters)
            {
                httpWebRequest.Headers[header.Name] = header.Value;
            }

            return httpWebRequest;
        }

        private static string BuildQueryStringParameters(WebRequestJob job)
        {
            if (job.BodyParameterType != BodyParameterType.FormUrlEncoded)
            {
                return string.Empty;
            }

            var parameters = job.BodyParameters.DeserializeObjectFromJson<List<HttpFormUrlEncodedParameter>>();
            var queryStringBuilder = new StringBuilder();

            foreach (var parameter in parameters)
            {
                if (queryStringBuilder.Length > 0)
                {
                    queryStringBuilder.Append("&");
                }
                queryStringBuilder.Append($"{WebUtility.UrlEncode(parameter.Name)}={WebUtility.UrlEncode(parameter.Value)}");
            }

            return queryStringBuilder.ToString();
        }

        private static bool IsBase64String(string value)
        {
            value = value.Trim();
            return (value.Length % 4 == 0) &&
                   Regex.IsMatch(value, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
        }
    }
}
