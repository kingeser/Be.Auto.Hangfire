using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions
{
    public class RecurringJobWebClient : WebClient
    {
        public string CallRequest(RecurringJobWebRequest job)
        {
            var baseUri = new Uri(job.HostName.TrimEnd('/'));
            var url = new Uri(baseUri, job.UrlPath.TrimStart('/')).ToString();

           
            if (!string.IsNullOrEmpty(job.HeaderParameters))
            {
                var headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(job.HeaderParameters);

                foreach (var header in headers)
                {
                    this.Headers[header.Key] = header.Value;
                }
            }

            try
            {
                switch (job.HttpMethod)
                {
                    case HttpMethodType.GET:
                    case HttpMethodType.DELETE:
                    case HttpMethodType.HEAD:
                    case HttpMethodType.OPTIONS:
                    case HttpMethodType.TRACE:
                        // Bu HTTP metotları body almaz, query string üzerinden parametreleri ekle
                        url = AppendQueryString(url, job.BodyParameters);
                        return ExecuteWebRequest(url, job.HttpMethod.ToString(), null);

                    case HttpMethodType.POST:
                    case HttpMethodType.PUT:
                    case HttpMethodType.PATCH:
                        var bodyContent = PrepareRequestBody(job);
                        return ExecuteWebRequest(url, job.HttpMethod.ToString(), bodyContent);

                    default:
                        throw new NotSupportedException($"HTTP method {job.HttpMethod} is not supported.");
                }
            }
            catch (WebException ex)
            {
                return HandleWebException(ex);
            }
        }

        private string PrepareRequestBody(RecurringJobWebRequest job)
        {
            if (string.IsNullOrEmpty(job.BodyParameters))
            {
                return null;
            }

            switch (job.BodyParameterType)
            {
                case BodyParameterType.Json:
                    this.Headers[HttpRequestHeader.ContentType] = "application/json";
                    return job.BodyParameters;

                case BodyParameterType.Xml:
                    this.Headers[HttpRequestHeader.ContentType] = "application/xml";
                    return job.BodyParameters;

                case BodyParameterType.FormUrlEncoded:
                    this.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    var formData = JsonConvert.DeserializeObject<Dictionary<string, string>>(job.BodyParameters);
                    var formEncodedContent = new StringBuilder();
                    foreach (var kvp in formData)
                    {
                        if (formEncodedContent.Length > 0)
                        {
                            formEncodedContent.Append("&");
                        }
                        formEncodedContent.Append($"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}");
                    }
                    return formEncodedContent.ToString();

                case BodyParameterType.PlainText:
                    this.Headers[HttpRequestHeader.ContentType] = "text/plain";
                    return job.BodyParameters;

                case BodyParameterType.FormData:
                    this.Headers[HttpRequestHeader.ContentType] = "multipart/form-data";
                    throw new NotSupportedException("FormData body type requires specific handling and is not implemented in this example.");

                default:
                    throw new NotSupportedException($"Body parameter type {job.BodyParameterType} is not supported.");
            }
        }

        private string AppendQueryString(string url, string bodyParameters)
        {
            if (string.IsNullOrEmpty(bodyParameters))
            {
                return url;
            }

            var queryData = JsonConvert.DeserializeObject<Dictionary<string, string>>(bodyParameters);
            var queryString = new StringBuilder();

            foreach (var kvp in queryData)
            {
                if (queryString.Length > 0)
                {
                    queryString.Append("&");
                }
                queryString.Append($"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}");
            }

            var separator = url.Contains("?") ? "&" : "?";
            return $"{url}{separator}{queryString}";
        }

        private string ExecuteWebRequest(string url, string method, string bodyContent)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            httpWebRequest.Method = method;

            foreach (var headerKey in this.Headers.AllKeys)
            {
                httpWebRequest.Headers[headerKey] = this.Headers[headerKey];
            }

            if (!string.IsNullOrEmpty(bodyContent) && (method == "PATCH" || method == "POST" || method == "PUT"))
            {
                var requestData = Encoding.UTF8.GetBytes(bodyContent);
                httpWebRequest.ContentLength = requestData.Length;

                using var requestStream = httpWebRequest.GetRequestStream();
                requestStream.Write(requestData, 0, requestData.Length);
            }

            using var response = (HttpWebResponse)httpWebRequest.GetResponse();
            using var stream = response.GetResponseStream();

            if (stream == null)
            {
                return "Stream not found!";
            }

            using var reader = new System.IO.StreamReader(stream);
            string responseData = reader.ReadToEnd();

            return responseData;
        }

        private string HandleWebException(WebException ex)
        {
            var responseStream = ex.Response?.GetResponseStream();
            if (responseStream == null) return ex.Message;

            using var reader = new System.IO.StreamReader(responseStream);
            return reader.ReadToEnd();
        }
    }
}
