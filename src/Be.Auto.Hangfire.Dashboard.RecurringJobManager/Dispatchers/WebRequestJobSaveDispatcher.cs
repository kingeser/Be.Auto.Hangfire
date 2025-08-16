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
using Hangfire.Common;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;


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
                                        errors.Add("The 'BodyParameters' field could not be deserialized into a valid list of 'HttpFormUrlEncodedParameter'.");
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
                                        errors.Add("The 'BodyParameters' field could not be deserialized into a valid list of 'HttpFormDataParameter'.");
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
        await context.Response.WriteAsync(@"
<!doctype html>
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
      <p>Send a JSON body that matches the <code>WebRequestJob</code> model below. The request describes <em>another</em> HTTP call the system will perform.</p>
    </section>

    <section class=""card"">
      <h2>Model</h2>
      <pre><code>class WebRequestJob {
  required string HostName;                 // e.g., ""https://api.example.com""
  required string UrlPath;                  // e.g., ""/v1/customers""
  required HttpMethodType HttpMethod;       // e.g., ""POST""
  required List&lt;HttpHeaderParameter&gt; HeaderParameters;
  required BodyParameterType BodyParameterType; // ""None"" | ""Json"" | ""Xml"" | ""FormUrlEncoded"" | ""FormData"" | ""PlainText""
  required string BodyParameters;           // payload; semantics depend on BodyParameterType
}

enum HttpMethodType { GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS, TRACE }

enum BodyParameterType { None, Json, Xml, FormUrlEncoded, FormData, PlainText }

class HttpHeaderParameter {
  string Name;
  string Value;
}</code></pre>

      <h3>Field requirements</h3>
      <table>
        <thead><tr><th>Field</th><th>Type</th><th>Required</th><th>Description</th></tr></thead>
        <tbody>
          <tr><td><code>HostName</code></td><td>string</td><td>Yes</td><td>Target host (no scheme). Example: <code>https://api.example.com</code>.</td></tr>
          <tr><td><code>UrlPath</code></td><td>string</td><td>Yes</td><td>Absolute path on the host. Example: <code>/v1/orders</code>.</td></tr>
          <tr><td><code>HttpMethod</code></td><td><code>HttpMethodType</code></td><td>Yes</td><td>HTTP verb to use when executing the outbound request.</td></tr>
          <tr><td><code>HeaderParameters</code></td><td>array of <code>HttpHeaderParameter</code></td><td>Yes</td><td>Headers to include (e.g., <code>Authorization</code>, <code>Content-Type</code>).</td></tr>
          <tr><td><code>BodyParameterType</code></td><td><code>BodyParameterType</code></td><td>Yes</td><td>How to interpret <code>BodyParameters</code>.</td></tr>
          <tr><td><code>BodyParameters</code></td><td>string</td><td>Yes</td><td>Payload as a string (JSON/XML/form-encoded/plain text). Use empty string for <code>None</code>.</td></tr>
        </tbody>
      </table>
    </section>

    <section class=""card"">
      <h2>JSON Schema (for validation)</h2>
      <pre><code>{
  ""$schema"": ""https://json-schema.org/draft/2020-12/schema"",
  ""type"": ""object"",
  ""required"": [
    ""HostName"",""UrlPath"",""HttpMethod"",
    ""HeaderParameters"",""BodyParameterType"",""BodyParameters""
  ],
  ""properties"": {
    ""HostName"": { ""type"": ""string"", ""minLength"": 1 },
    ""UrlPath"": { ""type"": ""string"", ""pattern"": ""^/.*"" },
    ""HttpMethod"": { ""type"": ""string"", ""enum"": [""GET"",""POST"",""PUT"",""DELETE"",""PATCH"",""HEAD"",""OPTIONS"",""TRACE""] },
    ""HeaderParameters"": {
      ""type"": ""array"",
      ""items"": {
        ""type"": ""object"",
        ""required"": [""Name"",""Value""],
        ""properties"": {
          ""Name"": { ""type"": ""string"", ""minLength"": 1 },
          ""Value"": { ""type"": ""string"" }
        },
        ""additionalProperties"": false
      }
    },
    ""BodyParameterType"": { ""type"": ""string"", ""enum"": [""None"",""Json"",""Xml"",""FormUrlEncoded"",""FormData"",""PlainText""] },
    ""BodyParameters"": { ""type"": ""string"" }
  },
  ""additionalProperties"": false
}</code></pre>
    </section>

    <section class=""card"">
      <h2>Request Examples</h2>
      <div class=""grid"">
        <div>
          <h3>cURL</h3>
          <pre><code>curl -X POST https://your-api-host.com/api/web-request-job   -H ""Content-Type: application/json""   -d '{
    ""HostName"": ""https://api.example.com"",
    ""UrlPath"": ""/v1/customers"",
    ""HttpMethod"": ""POST"",
    ""HeaderParameters"": [
      { ""Name"": ""Authorization"", ""Value"": ""Bearer &lt;token&gt;"" },
      { ""Name"": ""Content-Type"", ""Value"": ""application/json"" }
    ],
    ""BodyParameterType"": ""Json"",
    ""BodyParameters"": ""{\""name\"":\""Jane Doe\"",\""email\"":\""jane@example.com\""}""
  }'</code></pre>
        </div>
<br/>
        <div>
          <h3>.NET (HttpClient)</h3>
          <pre><code>var job = new {
  HostName = ""https://api.example.com"",
  UrlPath = ""/v1/customers"",
  HttpMethod = ""POST"",
  HeaderParameters = new [] {
    new { Name = ""Authorization"", Value = ""Bearer &lt;token&gt;"" },
    new { Name = ""Content-Type"", Value = ""application/json"" }
  },
  BodyParameterType = ""Json"",
  BodyParameters = ""{\""name\"":\""Jane Doe\"",\""email\"":\""jane@example.com\""}""
};

using var http = new HttpClient { BaseAddress = new Uri(""https://your-api-host.com"") };
var res = await http.PostAsJsonAsync(""/api/web-request-job"", job);
res.EnsureSuccessStatusCode();</code></pre>
        </div>
      </div>

      <h3>Body payloads by type</h3>
      <table>
        <thead><tr><th>BodyParameterType</th><th>BodyParameters (string)</th><th>Notes</th></tr></thead>
        <tbody>
          <tr>
            <td><code>None</code></td>
            <td><code>""""</code></td>
            <td>No body will be sent.</td>
          </tr>
          <tr>
            <td><code>Json</code></td>
            <td><code>{""name"":""Jane""}</code></td>
            <td>Ensure header includes <code>Content-Type: application/json</code>.</td>
          </tr>
          <tr>
            <td><code>Xml</code></td>
            <td><code>&lt;User&gt;&lt;Name&gt;Jane&lt;/Name&gt;&lt;/User&gt;</code></td>
            <td>Use <code>Content-Type: application/xml</code>.</td>
          </tr>
          <tr>
            <td><code>FormUrlEncoded</code></td>
            <td><code>name=Jane&amp;email=jane%40example.com</code></td>
            <td>Use <code>Content-Type: application/x-www-form-urlencoded</code>.</td>
          </tr>
          <tr>
            <td><code>FormData</code></td>
            <td><code>----boundary... (multipart body as text)</code></td>
            <td>Provide the full multipart body as a string; include correct <code>Content-Type</code> with boundary.</td>
          </tr>
          <tr>
            <td><code>PlainText</code></td>
            <td><code>Any raw text...</code></td>
            <td>Use <code>Content-Type: text/plain</code>.</td>
          </tr>
        </tbody>
      </table>
    </section>

    <section class=""card"">
      <h2>Response</h2>
      <p>Typical responses from <code>/api/web-request-job</code>:</p>
      <table>
        <thead><tr><th>Status</th><th>Meaning</th></tr></thead>
        <tbody>
          <tr><td class=""status ok"">202 Accepted</td><td>Job accepted for processing.</td></tr>
          <tr><td class=""status ok"">200 OK</td><td>Job created and executed immediately (if synchronous).</td></tr>
          <tr><td class=""status warn"">400 Bad Request</td><td>Validation failed (see message for missing or invalid fields).</td></tr>
          <tr><td class=""status warn"">415 Unsupported Media Type</td><td><code>BodyParameterType</code> and <code>Content-Type</code> mismatch.</td></tr>
          <tr><td class=""status bad"">500 Internal Server Error</td><td>Unexpected error.</td></tr>
        </tbody>
      </table>
    </section>

    <section class=""card"">
      <h2>Validation Rules</h2>
      <ul>
        <li><strong>HostName</strong> must be present and not contain protocol (no <code>http://</code> or <code>https://</code>).</li>
        <li><strong>UrlPath</strong> must start with <kbd>/</kbd>.</li>
        <li><strong>HttpMethod</strong> must be one of: GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS, TRACE.</li>
        <li><strong>HeaderParameters</strong> each require non-empty <code>Name</code>. Duplicate names will be sent in order provided.</li>
        <li><strong>BodyParameterType</strong> dictates how <strong>BodyParameters</strong> is forwarded and which <code>Content-Type</code> is expected.</li>
        <li>If <strong>BodyParameterType</strong> is <code>None</code>, <strong>BodyParameters</strong> should be an empty string.</li>
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
    { ""Name"": ""Authorization"", ""Value"": ""Bearer &lt;token&gt;"" }
  ],
  ""BodyParameterType"": ""None"",
  ""BodyParameters"": """"
}</code></pre>
    </section>

    <footer class=""card"">
      <p>© Web Request Job API – README</p>
    </footer>
  </main>
</body>
</html>");
    }
}