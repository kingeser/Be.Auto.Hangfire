using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;
using Hangfire;
using Hangfire.Dashboard;


namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dispatchers;



internal sealed class WebRequestJobSaveDispatcher : IDashboardDispatcher
{
    public async Task Dispatch(DashboardContext context)
    {

        switch (context.Request.Method)
        {
            case nameof(HttpMethodType.GET):
                {
                    await WriteDocumentAsync(context);
                }
                break;
            case nameof(HttpMethodType.POST):
                {
                    if (context is AspNetCoreDashboardContext aspNetCoreDashboardContext)
                    {
                        using var reader = new StreamReader(aspNetCoreDashboardContext.HttpContext.Request.Body, Encoding.UTF8, true, 1024, leaveOpen: true);

                        var body = await reader.ReadToEndAsync();

                        if (body.TryDeserializeObjectFromJson<WebRequestJob>(out var webRequestJob))
                        {
                            webRequestJob.UrlPath ??= string.Empty;
                            webRequestJob.BodyParameters = $"{webRequestJob.BodyParameters}".UnescapeMulti();

                            var errors = new List<string>();


                            if (string.IsNullOrEmpty(webRequestJob.HostName))
                                errors.Add("The 'HostName' field cannot be null or empty.");


                            if(!Uri.TryCreate(webRequestJob.HostName,UriKind.RelativeOrAbsolute,out var _))
                                errors.Add("The 'HostName' invalid Uri!");

                           
                            // BodyParameters validation based on BodyParameterType
                            switch (webRequestJob.BodyParameterType)
                            {
                                case BodyParameterType.Json:
                                    if (!webRequestJob.BodyParameters.IsValidJson())
                                        errors.Add("The 'BodyParameters' field contains invalid JSON.");
                                    break;

                                case BodyParameterType.Xml:
                                    if (!webRequestJob.BodyParameters.IsValidXml())
                                        errors.Add("The 'BodyParameters' field contains invalid XML.");
                                    break;

                                case BodyParameterType.FormUrlEncoded:
                                    if (!webRequestJob.BodyParameters.TryDeserializeObjectFromJson<List<HttpFormUrlEncodedParameter>>(out var formUrlEncodedParameters))
                                        errors.Add("The 'BodyParameters' field could not be deserialized into a valid list of 'HttpFormUrlEncodedParameter'.\r\n Example : [{\"Name\":\"username\",\"Value\":\"burak.eser\"},{\"Name\":\"password\",\"Value\":\"123456\"},{\"Name\":\"rememberMe\",\"Value\":\"true\"}]\r\n");
                                    else
                                    {
                                        if (!formUrlEncodedParameters.Any())
                                            errors.Add("The 'BodyParameters' list is empty.");
                                        if (formUrlEncodedParameters.Exists(t => string.IsNullOrEmpty(t.Name) || string.IsNullOrEmpty(t.Value)))
                                            errors.Add("The 'BodyParameters' list contains entries with empty 'Name' or 'Value' fields.");
                                    }
                                    break;

                                case BodyParameterType.FormData:
                                    if (!webRequestJob.BodyParameters.TryDeserializeObjectFromJson<List<HttpFormDataParameter>>(out var formDataParameters))
                                        errors.Add("The 'BodyParameters' field could not be deserialized into a valid list of 'HttpFormDataParameter'.\r\n Example : [{\"Name\":\"username\",\"Value\":\"burak.eser\",\"ContentType\":\"text/plain\"},{\"Name\":\"profilePicture\",\"Value\":\"BASE64_STRING_HERE\",\"ContentType\":\"image/png\"}]\r\n");
                                    else
                                    {
                                        if (!formDataParameters.Any())
                                            errors.Add("The 'BodyParameters' list is empty.");
                                        if (formDataParameters.Exists(t => string.IsNullOrEmpty(t.Name) || string.IsNullOrEmpty(t.Value) || string.IsNullOrEmpty(t.ContentType)))
                                            errors.Add("The 'BodyParameters' list contains entries with empty 'Name', 'Value', or 'ContentType' fields.");
                                    }
                                    break;

                                case BodyParameterType.PlainText:
                                    if (string.IsNullOrEmpty(webRequestJob.BodyParameters))
                                        errors.Add("The 'BodyParameters' field cannot be null or empty.");
                                    break;
                            }

                            // Eğer herhangi bir hata varsa tek exception fırlat
                            if (errors.Any())
                            {
                                throw new InvalidOperationException("Job registration failed:\n" + string.Join("\n", errors));
                            }


                            BackgroundJob.Enqueue<RecurringJobWebClient>(client => client.CallRequestAsync(webRequestJob));

                          
                        }
                        else
                        {

                            throw new InvalidOperationException("Invalid request!Please read documentation.");
                        }


                    }
                    else
                    {
                        throw new NotSupportedException();
                    }

                    break;
                }
        }
    }





    private static async Task WriteDocumentAsync(DashboardContext context)
    {
        await context.Response.WriteAsync(@"<!doctype html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
  <title>Web Request Job API – README</title>
  <style>
    :root {
      --bg: #0b0f19;
      --card: #121826;
      --muted: #94a3b8;
      --text: #e5e7eb;
      --accent: #60a5fa;
      --code: #0f172a;
      --border: #1f2937;
      --good: #34d399;
      --warn: #fbbf24;
      --bad: #f87171;
    }
    html, body { background: var(--bg); color: var(--text); font: 16px/1.6 system-ui, -apple-system, Segoe UI, Roboto, Ubuntu, Cantarell, Noto Sans, Helvetica, Arial; margin: 0; padding: 0; }
    a { color: var(--accent); text-decoration: none; }
    a:hover { text-decoration: underline; }
    .container { max-width: 1100px; margin: 0 auto; padding: 2rem; }
    .title { font-size: 2rem; font-weight: 800; margin: 0 0 1rem; letter-spacing: .3px; }
    .subtitle { color: var(--muted); margin-top: -.4rem; }
    .card { background: linear-gradient(180deg, rgba(255,255,255,.02), rgba(255,255,255,.00)); border: 1px solid var(--border); border-radius: 16px; padding: 1.25rem; margin: 1rem 0 1.5rem; }
    h2 { font-size: 1.25rem; margin: 1.5rem 0 .75rem; }
    h3 { font-size: 1.05rem; margin: 1rem 0 .5rem; }
    code, pre { font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, ""Liberation Mono"", monospace; font-size: .95rem; }
    pre { background: var(--code); padding: 1rem; border-radius: 12px; overflow: auto; border: 1px solid var(--border); }
    kbd { background: var(--code); padding: .15rem .35rem; border-radius: 6px; border: 1px solid var(--border); }
    table { width: 100%; border-collapse: collapse; margin: .5rem 0 1rem; }
    th, td { border: 1px solid var(--border); padding: .6rem .7rem; text-align: left; vertical-align: top; }
    th { background: #141c2e; }
    .pill { display: inline-block; padding: .15rem .5rem; border-radius: 999px; border: 1px solid var(--border); background: #141c2e; color: var(--text); font-size: .85rem; }
    .status { font-weight: 700; }
    .ok { color: var(--good); }
    .warn { color: var(--warn); }
    .bad { color: var(--bad); }
    .grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
    @media (max-width: 900px) { .grid { grid-template-columns: 1fr; } }
  </style>
</head>
<body>
  <main class=""container"">
    <header>
      <h1 class=""title"">Web Request Job API - Fire And Forget</h1>
      <p class=""subtitle"">Submit an outbound web request definition to be executed by the system.</p>
    </header>

    <section class=""card"">
      <h2>Endpoint</h2>
      <table>
        <tr><th>Method</th><td><span class=""pill"">POST</span></td></tr>
        <tr><th>Path</th><td><code>/api/web-request-job</code></td></tr>
        <tr><th>Content-Type</th><td><code>application/json</code></td></tr>
        <tr><th>Authentication</th><td>As configured by your environment (e.g., API key / Bearer). Include details in <code>HeaderParameters</code> if needed.</td></tr>
      </table>
    </section>

    <section class=""card"">
      <h2>Model</h2>
      <pre><code>class WebRequestJob {
  string HostName;                 // e.g., ""https://api.example.com""
  string UrlPath;                  // e.g., ""/v1/customers""
  HttpMethodType HttpMethod;       // e.g., ""POST""
  List&lt;HttpHeaderParameter&gt; HeaderParameters;
  BodyParameterType BodyParameterType; // ""None"" | ""Json"" | ""Xml"" | ""FormUrlEncoded"" | ""FormData"" | ""PlainText""
  string BodyParameters;           // JSON string. If FormUrlEncoded or FormData, serialize corresponding class array as JSON
}

class HttpHeaderParameter {
  string Name;
  string Value;
}

class HttpFormUrlEncodedParameter {
  string Name;
  string Value;
}

class HttpFormDataParameter {
  string Name;
  string Value;        // Base64 string if file, plain value if normal field
  string ContentType;  // e.g., ""text/plain"", ""image/png"", ""application/pdf""
}

enum HttpMethodType { GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS, TRACE }
enum BodyParameterType { None, Json, Xml, FormUrlEncoded, FormData, PlainText }</code></pre>
    </section>

    <section class=""card"">
      <h2>Request Examples</h2>
    
          <h3>cURL</h3>
          <pre><code>curl -X POST https://your-api-host.com/api/web-request-job \
  -H ""Content-Type: application/json"" \
  -d '{
    ""HostName"": ""https://api.example.com"",
    ""UrlPath"": ""/v1/customers"",
    ""HttpMethod"": ""POST"",
    ""HeaderParameters"": [
      { ""Name"": ""Authorization"", ""Value"": ""Bearer <token>"" },
      { ""Name"": ""Content-Type"", ""Value"": ""application/json"" }
    ],
    ""BodyParameterType"": ""Json"",
    ""BodyParameters"": {""name"":""Jane Doe"",""email"":""jane@example.com""}
  }'

# FormUrlEncoded example (BodyParameters is JSON string of array)
curl -X POST https://your-api-host.com/api/web-request-job \
  -H ""Content-Type: application/json"" \
  -d '{
    ""HostName"": ""https://api.example.com"",
    ""UrlPath"": ""/v1/login"",
    ""HttpMethod"": ""POST"",
    ""HeaderParameters"": [
      { ""Name"": ""Content-Type"", ""Value"": ""application/x-www-form-urlencoded"" }
    ],
    ""BodyParameterType"": ""FormUrlEncoded"",
    ""BodyParameters"": [{""Name"":""username"",""Value"":""jane""},{""Name"":""password"",""Value"":""123456""}]
  }'

# FormData example (BodyParameters is JSON string of array)
curl -X POST https://your-api-host.com/api/web-request-job \
  -H ""Content-Type: application/json"" \
  -d '{
    ""HostName"": ""https://api.example.com"",
    ""UrlPath"": ""/v1/upload"",
    ""HttpMethod"": ""POST"",
    ""HeaderParameters"": [
      { ""Name"": ""Authorization"", ""Value"": ""Bearer <token>"" }
    ],
    ""BodyParameterType"": ""FormData"",
    ""BodyParameters"": [{""Name"":""username"",""Value"":""burak.eser"",""ContentType"":""text/plain""},{""Name"":""profilePicture"",""Value"":""BASE64_STRING_HERE"",""ContentType"":""image/png""}]
  }'</code></pre>
     
          <h3>.NET (HttpClient)</h3>
          <pre><code>// Json example
var jobJson = new {
  HostName = ""https://api.example.com"",
  UrlPath = ""/v1/customers"",
  HttpMethod = ""POST"",
  HeaderParameters = new [] {
    new { Name = ""Authorization"", Value = ""Bearer <token>"" },
    new { Name = ""Content-Type"", Value = ""application/json"" }
  },
  BodyParameterType = ""Json"",
  BodyParameters = {""name"":""Jane Doe"",""email"":""jane@example.com""}
};
await http.PostAsJsonAsync(""/api/web-request-job"", jobJson);

// FormUrlEncoded example
var jobFormUrl = new {
  HostName = ""https://api.example.com"",
  UrlPath = ""/v1/login"",
  HttpMethod = ""POST"",
  HeaderParameters = new [] {
    new { Name = ""Content-Type"", Value = ""application/x-www-form-urlencoded"" }
  },
  BodyParameterType = ""FormUrlEncoded"",
  BodyParameters = [{""Name"":""username"",""Value"":""jane""},{""Name"":""password"",""Value"":""123456""}]
};
await http.PostAsJsonAsync(""/api/web-request-job"", jobFormUrl);

// FormData example
var jobFormData = new {
  HostName = ""https://api.example.com"",
  UrlPath = ""/v1/upload"",
  HttpMethod = ""POST"",
  HeaderParameters = new [] {
    new { Name = ""Authorization"", Value = ""Bearer <token>"" }
  },
  BodyParameterType = ""FormData"",
  BodyParameters = [{""Name"":""username"",""Value"":""burak.eser"",""ContentType"":""text/plain""},{""Name"":""profilePicture"",""Value"":""BASE64_STRING_HERE"",""ContentType"":""image/png""}]
};
await http.PostAsJsonAsync(""/api/web-request-job"", jobFormData);</code></pre>
    
    </section>

    <section class=""card"">
      <h2>Response</h2>
      <table>
        <thead><tr><th>Status</th><th>Meaning</th></tr></thead>
        <tbody>
          <tr><td class=""status ok"">202 Accepted</td><td>Job accepted for processing.</td></tr>
          <tr><td class=""status ok"">200 OK</td><td>Job created and executed immediately (if synchronous).</td></tr>
          <tr><td class=""status warn"">400 Bad Request</td><td>Validation failed (see message for missing or invalid fields).</td></tr>
          <tr><td class=""status warn"">415 Unsupported Media Type</td><td>BodyParameterType and Content-Type mismatch.</td></tr>
          <tr><td class=""status bad"">500 Internal Server Error</td><td>Unexpected error.</td></tr>
        </tbody>
      </table>
    </section>

    <section class=""card"">
      <h2>Validation Rules</h2>
      <ul>
        <li>HostName must include protocol (http:// or https://).</li>
        <li>UrlPath must start with <kbd>/</kbd>.</li>
        <li>HttpMethod must be one of: GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS, TRACE.</li>
        <li>HeaderParameters each require non-empty Name. Duplicate names will be sent in order provided.</li>
        <li>BodyParameterType dictates how BodyParameters is interpreted.</li>
        <li>If BodyParameterType is None, BodyParameters should be an empty string.</li>
      </ul>
    </section>

    <section class=""card"">
      <h2>Complete Example (POST /api/web-request-job)</h2>
      <pre><code>POST /api/web-request-job HTTP/1.1
Host: your-api-host.com
Content-Type: application/json

{
  ""HostName"": ""https://api.example.com"",
  ""UrlPath"": ""/v1/orders/123"",
  ""HttpMethod"": ""GET"",
  ""HeaderParameters"": [
    { ""Name"": ""Authorization"", ""Value"": ""Bearer <token>"" }
  ],
  ""BodyParameterType"": ""None"",
  ""BodyParameters"": """"
}</code></pre>
    </section>

<section class=""card"">
  <h2>Client Usage</h2>
  <p>To simplify sending web request jobs, you can use the <code>HangfireWebRequestJobApiClient</code>. 
     It supports all body types and automatically serializes the data as required.</p>

  <h3>Instantiation</h3>
  <pre><code>var client = new HangfireWebRequestJobApiClient(new Uri(""https://your-api-host.com""));
client.Headers.Add(new HttpHeaderParameter { Name = ""Authorization"", Value = ""Bearer &lt;token&gt;"" });</code></pre>

  <h3>JSON Body Example</h3>
  <pre><code>var jobJson = new WebRequestJobBodyJson
{
    Uri = new Uri(""https://api.example.com/v1/customers""),
    Method = HttpMethodType.POST,
    HeaderParameters = new []
    {
        new HttpHeaderParameter { Name = ""Content-Type"", Value = ""application/json"" }
    },
    BodyParameters = new { name = ""Jane Doe"", email = ""jane@example.com"" }
};

var response = await client.AddAsync(jobJson);
Console.WriteLine(response.StatusCode);</code></pre>

  <h3>XML Body Example</h3>
  <pre><code>var xmlDoc = new XmlDocument();
xmlDoc.LoadXml(""&lt;Customer&gt;&lt;Name&gt;Jane&lt;/Name&gt;&lt;Email&gt;jane@example.com&lt;/Email&gt;&lt;/Customer&gt;"");

var jobXml = new WebRequestJobBodyXml
{
    Uri = new Uri(""https://api.example.com/v1/customers""),
    Method = HttpMethodType.POST,
    HeaderParameters = new []
    {
        new HttpHeaderParameter { Name = ""Content-Type"", Value = ""application/xml"" }
    },
    BodyParameters = xmlDoc
};

var response = await client.AddAsync(jobXml);</code></pre>

  <h3>FormUrlEncoded Example</h3>
  <pre><code>var jobFormUrl = new WebRequestJobBodyFormUrlEncoded
{
    Uri = new Uri(""https://api.example.com/v1/login""),
    Method = HttpMethodType.POST,
    HeaderParameters = new []
    {
        new HttpHeaderParameter { Name = ""Content-Type"", Value = ""application/x-www-form-urlencoded"" }
    },
    BodyParameters = new[]
    {
        new HttpFormUrlEncodedParameter { Name = ""username"", Value = ""jane"" },
        new HttpFormUrlEncodedParameter { Name = ""password"", Value = ""123456"" }
    }
};

var response = await client.AddAsync(jobFormUrl);</code></pre>

  <h3>FormData Example</h3>
  <pre><code>var jobFormData = new WebRequestJobBodyFormData
{
    Uri = new Uri(""https://api.example.com/v1/upload""),
    Method = HttpMethodType.POST,
    HeaderParameters = new []
    {
        new HttpHeaderParameter { Name = ""Authorization"", Value = ""Bearer &lt;token&gt;"" }
    },
    BodyParameters = new[]
    {
        new HttpFormDataParameter { Name = ""username"", Value = ""burak.eser"", ContentType = ""text/plain"" },
        new HttpFormDataParameter { Name = ""profilePicture"", Value = ""BASE64_STRING_HERE"", ContentType = ""image/png"" }
    }
};

var response = await client.AddAsync(jobFormData);</code></pre>

  <h3>PlainText Body Example</h3>
  <pre><code>var jobText = new WebRequestJobBodyPlainText
{
    Uri = new Uri(""https://api.example.com/v1/notes""),
    Method = HttpMethodType.POST,
    HeaderParameters = new []
    {
        new HttpHeaderParameter { Name = ""Content-Type"", Value = ""text/plain"" }
    },
    BodyParameters = ""This is a plain text note.""
};

var response = await client.AddAsync(jobText);</code></pre>

  <h3>None Body Example</h3>
  <pre><code>var jobNone = new WebRequestJobBodyNone
{
    Uri = new Uri(""https://api.example.com/v1/ping""),
    Method = HttpMethodType.GET,
    HeaderParameters = new []
    {
        new HttpHeaderParameter { Name = ""Authorization"", Value = ""Bearer &lt;token&gt;"" }
    }
};

var response = await client.AddAsync(jobNone);</code></pre>

  <p>All responses are returned as <code>WebRequestJobResponse</code>, containing <code>StatusCode</code>, <code>ExceptionCode</code>, and <code>ExceptionMessage</code>.</p>
</section>


    <footer class=""card"">
      <p>© Web Request Job API – README</p>
    </footer>
  </main>
</body>
</html>

");
    }
}