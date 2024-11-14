using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core
{
    internal class RecurringJobWebClient
    {
        public async Task<object> CallRequestAsync(WebRequestJob job)
        {

            var baseUri = new Uri(job.HostName.TrimEnd('/'));

            var url = new Uri(baseUri, job.UrlPath.TrimStart('/')).ToString();

            if (IsSimpleRequest(job.HttpMethod))
            {
                var simpleResponse = await HandleSimpleRequestAsync(job, url);

                return new
                {
                    Response = simpleResponse.Item1,
                    ResponseString = simpleResponse.Item2
                };
            }

            var response = IsComplexRequest(job.HttpMethod) ? await HandleComplexRequestAsync(job, url) : null;

            if (response == null) return null;

            return new
            {
                Response = response.Item1,
                ResponseString = response.Item2
            };

        }


        private static bool IsSimpleRequest(HttpMethodType method) => method is HttpMethodType.GET or HttpMethodType.DELETE or HttpMethodType.HEAD or HttpMethodType.OPTIONS or HttpMethodType.TRACE;

        private static async Task<Tuple<HttpWebResponse, string>> HandleSimpleRequestAsync(WebRequestJob job, string url)
        {
            var queryString = BuildQueryStringParameters(job);

            if (!url.EndsWith("?"))
                url += "?";

            url += queryString;

            var httpWebRequest = CreateWebRequest(job, url);

            return await GetResponseAsync(httpWebRequest);
        }

        private static bool IsComplexRequest(HttpMethodType method) => method is HttpMethodType.POST or HttpMethodType.PUT or HttpMethodType.PATCH;

        private static async Task<Tuple<HttpWebResponse, string>> HandleComplexRequestAsync(WebRequestJob job, string url)
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

        private static async Task<Tuple<HttpWebResponse, string>> SendRequestWithEmptyBodyAsync(HttpWebRequest httpWebRequest)
        {
            httpWebRequest.ContentLength = 0;
            return await GetResponseAsync(httpWebRequest);
        }

        private static async Task<Tuple<HttpWebResponse, string>> SendRequestWithBodyAsync(HttpWebRequest httpWebRequest, string body, string contentType)
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

        private static async Task<Tuple<HttpWebResponse, string>> SendFormUrlEncodedRequestAsync(HttpWebRequest httpWebRequest, string bodyParameters)
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

        private static async Task<Tuple<HttpWebResponse, string>> SendMultipartFormDataRequestAsync(HttpWebRequest httpWebRequest, string bodyParameters)
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

        private static async Task<Tuple<HttpWebResponse, string>> GetResponseAsync(HttpWebRequest httpWebRequest)
        {
            try
            {
                var response = (HttpWebResponse)await httpWebRequest.GetResponseAsync();

                if (response.StatusCode != HttpStatusCode.OK) return new Tuple<HttpWebResponse, string>(response, string.Empty);

                using var stream = response.GetResponseStream();

                if (stream == null) return new Tuple<HttpWebResponse, string>(response, string.Empty);

                using var reader = new StreamReader(stream);

                var responseBody = await reader.ReadToEndAsync();

                return new Tuple<HttpWebResponse, string>(response, responseBody);
            }
            catch (Exception ex)
            {
                if (ex is not WebException { Response: HttpWebResponse errorResponse })
                    throw;

                using var reader = new StreamReader(errorResponse.GetResponseStream() ?? Stream.Null);

                var errorContent = $"{(await reader.ReadToEndAsync())}";

                if (!IsHtmlContent(errorContent))
                {
                    throw new RecurringJobException($"{errorResponse.StatusCode} : {errorResponse.StatusDescription} > {ex.Message}", ex);

                }

                var errorDetails = ExtractErrorDetails(errorContent);


                throw new RecurringJobException($"{errorResponse.StatusCode} : {errorResponse.StatusDescription} > {ex.Message}", new WebException(errorDetails, ex));


            }
        }

        private static bool IsHtmlContent(string content)
        {
            return Regex.IsMatch(content, @"<[a-z][\s\S]*>", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// For ServiceStack etc. html response errors
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private static string ExtractErrorDetails(string content)
        {
            const string pattern = @"\{(?:[^{}]|(?<Open>\{)|(?<-Open>\}))*(?(Open)(?!))\}|\[(?:[^\[\]]|(?<Open>\[)|(?<-Open>\]))*(?(Open)(?!))\]";

            var regex = new Regex(pattern, RegexOptions.Singleline);
            var culture = new CultureInfo("en-US");
            return string.Join("\r\n", regex.Matches(content)
                .Cast<Match>()
                .Where(m => m.Success)
                .Where(t=>!string.IsNullOrEmpty(t.Value))
                .Where(a => a.Value.ToLower(culture).Contains("error") || a.Value.Contains("exception") ||a.Value.Contains("fail") || a.Value.Contains("fatal")|| a.Value.Contains("unexpected"))
                .Select(a => a.Value)
                .Distinct());
        }
        private static HttpWebRequest CreateWebRequest(WebRequestJob job, string url)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            httpWebRequest.Timeout = Convert.ToInt32(Options.Instance.WebRequestJob?.TimeOut.TotalMilliseconds ?? TimeSpan.FromSeconds(30).TotalMilliseconds);

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
            if (string.IsNullOrEmpty(value)) return false;

            value = value.Trim();

            return (value.Length % 4 == 0) && Regex.IsMatch(value, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);

        }
    }
}
