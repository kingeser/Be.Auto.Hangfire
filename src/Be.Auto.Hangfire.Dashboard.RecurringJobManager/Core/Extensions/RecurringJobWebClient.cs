using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;
using System;
using System.Linq;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions
{
    public class RecurringJobWebClient
    {
        public string CallRequest(WebRequestJob job)
        {
            var baseUri = new Uri(job.HostName.TrimEnd('/'));

            var url = new Uri(baseUri, job.UrlPath.TrimStart('/')).ToString();

            if (IsSimpleRequest(job.HttpMethod))
            {
                return HandleSimpleRequest(job, url);
            }

            return IsComplexRequest(job.HttpMethod) ? HandleComplexRequest(job, url) : string.Empty;
        }

        private static bool IsSimpleRequest(HttpMethodType method) => method is HttpMethodType.GET or HttpMethodType.DELETE or HttpMethodType.HEAD or HttpMethodType.OPTIONS or HttpMethodType.TRACE;

        private static string HandleSimpleRequest(WebRequestJob job, string url)
        {
            var queryString = BuildQueryStringParameters(job);

            if (!url.EndsWith("?"))
                url += "?";

            url += queryString;

            var httpWebRequest = CreateWebRequest(job, url);

            using var response = (HttpWebResponse)httpWebRequest.GetResponse();
            using var reader = new StreamReader(response.GetResponseStream() ?? Stream.Null);
            return reader.ReadToEnd();
        }

        private static bool IsComplexRequest(HttpMethodType method) => method is HttpMethodType.POST or HttpMethodType.PUT or HttpMethodType.PATCH;

        private static string HandleComplexRequest(WebRequestJob job, string url)
        {
            var httpWebRequest = CreateWebRequest(job, url);

            return job.BodyParameterType switch
            {
                BodyParameterType.None => SendRequestWithEmptyBody(httpWebRequest),
                BodyParameterType.Json => SendRequestWithBody(httpWebRequest, job.BodyParameters, "application/json"),
                BodyParameterType.Xml => SendRequestWithBody(httpWebRequest, job.BodyParameters, "application/xml"),
                BodyParameterType.FormUrlEncoded => SendFormUrlEncodedRequest(httpWebRequest, job.BodyParameters),
                BodyParameterType.FormData => SendMultipartFormDataRequest(httpWebRequest, job.BodyParameters),
                BodyParameterType.PlainText => SendRequestWithBody(httpWebRequest, job.BodyParameters, "text/plain"),
                _ => string.Empty
            };
        }

        private static string SendRequestWithEmptyBody(HttpWebRequest httpWebRequest)
        {
            httpWebRequest.ContentLength = 0;
            using var response = (HttpWebResponse)httpWebRequest.GetResponse();
            using var responseStream = new StreamReader(response.GetResponseStream() ?? Stream.Null);
            return responseStream.ReadToEnd();
        }

        private static string SendRequestWithBody(HttpWebRequest httpWebRequest, string body, string contentType)
        {
            httpWebRequest.ContentType = contentType;
            using (var requestStream = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                if (!string.IsNullOrEmpty(body))
                {
                    requestStream.Write(body);
                }
            }
            return GetResponseText(httpWebRequest);
        }

        private static string SendFormUrlEncodedRequest(HttpWebRequest httpWebRequest, string bodyParameters)
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

            using (var requestStream = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                requestStream.Write(formData.ToString());
            }

            return GetResponseText(httpWebRequest);
        }

        private static string SendMultipartFormDataRequest(HttpWebRequest httpWebRequest, string bodyParameters)
        {
            var boundary = "------------------------" + DateTime.Now.Ticks.ToString("x");
            httpWebRequest.ContentType = "multipart/form-data; boundary=" + boundary;

            var parameters = bodyParameters.DeserializeObjectFromJson<List<HttpFormDataParameter>>();

            using var requestStream = httpWebRequest.GetRequestStream();

            foreach (var param in parameters)
            {
                var header = $"--{boundary}\r\nContent-Disposition: form-data; name=\"{param.Name}\"\r\n" +
                             $"Content-Type: {param.ContentType}\r\n\r\n";
                var headerBytes = Encoding.UTF8.GetBytes(header);

                requestStream.Write(headerBytes, 0, headerBytes.Length);

                if (!string.IsNullOrEmpty(param.Value))
                {
                    var dataBytes = IsBase64String(param.Value)
                        ? Convert.FromBase64String(param.Value)
                        : Encoding.UTF8.GetBytes(param.Value);
                    requestStream.Write(dataBytes, 0, dataBytes.Length);
                }

                var newlineBytes = "\r\n"u8.ToArray();
                requestStream.Write(newlineBytes, 0, newlineBytes.Length);
            }

            return GetResponseText(httpWebRequest);
        }

        private static string GetResponseText(HttpWebRequest httpWebRequest)
        {
            using var response = (HttpWebResponse)httpWebRequest.GetResponse();
            using var responseStream = new StreamReader(response.GetResponseStream() ?? Stream.Null);
            return responseStream.ReadToEnd();
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
