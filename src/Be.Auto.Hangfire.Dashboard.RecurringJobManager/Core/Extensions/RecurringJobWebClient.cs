using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;
using Newtonsoft.Json;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;

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
                this.Headers.Add(header.Key, header.Value);
            }
        }

        var bodyContent = PrepareRequestBody(job);

        try
        {
            switch (job.HttpMethod)
            {
                case HttpMethodType.GET:
                    return DownloadString(url);


                case HttpMethodType.POST:
                    return UploadString(url, "POST", bodyContent ?? string.Empty);


                case HttpMethodType.PUT:
                    return UploadString(url, "PUT", bodyContent ?? string.Empty);


                case HttpMethodType.DELETE:
                    return UploadString(url, "DELETE", string.Empty);


                case HttpMethodType.PATCH:
                    return ExecuteWebRequest(url, "PATCH", bodyContent);


                case HttpMethodType.HEAD:
                    return ExecuteWebRequest(url, "HEAD", null);

                case HttpMethodType.OPTIONS:
                    return ExecuteWebRequest(url, "OPTIONS", null);


                case HttpMethodType.TRACE:
                    return ExecuteWebRequest(url, "TRACE", null);


                default:
                    throw new NotSupportedException($"HTTP method {job.HttpMethod} is not supported.");
            }
        }
        catch (WebException ex)
        {
            var stream = ex.Response?.GetResponseStream();

            if (stream == null) return ex.GetAllMessages();
            using var reader = new System.IO.StreamReader(stream);
            string errorResponse = reader.ReadToEnd();
            return errorResponse;
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

    private string ExecuteWebRequest(string url, string method, string bodyContent)
    {
        var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

        httpWebRequest.Method = method;

        foreach (var headerKey in this.Headers.AllKeys)
        {
            httpWebRequest.Headers.Add(headerKey, this.Headers[headerKey]);
        }

        if (!string.IsNullOrEmpty(bodyContent) && method is "PATCH" or "POST" or "PUT")
        {
            var requestData = Encoding.UTF8.GetBytes(bodyContent);

            httpWebRequest.ContentLength = requestData.Length;

            using var requestStream = httpWebRequest.GetRequestStream();

            requestStream.Write(requestData, 0, requestData.Length);
        }

        using var response = (HttpWebResponse)httpWebRequest.GetResponse();

        var stream = response.GetResponseStream();

        if (stream == null) return "Stream not found!";

        using var reader = new System.IO.StreamReader(stream);
        string responseData = reader.ReadToEnd();

        return responseData;
    }
}