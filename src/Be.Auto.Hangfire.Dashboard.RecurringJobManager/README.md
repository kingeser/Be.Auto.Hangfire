# Be.Auto.Hangfire.Dashboard.RecurringJobManager

## Overview

**Be.Auto.Hangfire.Dashboard.RecurringJobManager** is a specialized library for managing recurring jobs in Hangfire-based projects. It allows you to configure and manage various types of jobs, including method calls and web requests, directly from the Hangfire Dashboard. This enables all job management tasks to be handled through the dashboard interface without needing to manually add jobs in code.

## Version 1.2.1 Highlights

- **Bug Fixes**: Known issues in previous versions have been resolved.
- **Prevent Concurrent Execution Improvements**: Concurrent execution prevention has been improved on a per-job basis.
- **Web Request Jobs API**: Fire-and-forget WebRequestJobs can now be added externally via a dedicated API.
- **HangfireWebRequestJobApiClient**: A new client class added for easier programmatic submission of WebRequestJobs.
- **Application-Wide Improvements**: General performance and usability improvements across the system.

## Key Features

- **Automatic Type and Method Detection**: Automatically detects types and methods within the project’s assembly, allowing you to input parameters directly through the Hangfire Dashboard.
- **Dependency Injection (DI) Support**: Types registered with DI are automatically resolved.
- **Comprehensive Job Management**: Supports MethodCall and WebRequestJob types, with full control over job creation, editing, and deletion through the dashboard.
- **HTTP Request Jobs**: Allows detailed configuration of HTTP requests, including headers, body, and query parameters, directly through the dashboard. It also prevents collisions by avoiding the duplication of jobs with identical parameters.
- **Fire-and-Forget API**: External systems can enqueue WebRequest jobs directly as fire-and-forget tasks via the new API endpoint.
- **Client Support**: Provides a convenient `HangfireWebRequestJobApiClient` to simplify adding WebRequestJobs programmatically.

## Installation

1. **Add the NuGet Package**:

```bash
Install-Package Be.Auto.Hangfire.Dashboard.RecurringJobManager
```

2. **Configure Services**: In your `Startup.cs`, add the following configuration:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHangfire(config => config
        .UseSqlServerStorage(Configuration.GetConnectionString("HangfireConnection"))
        .UseDashboardRecurringJobManager(option =>
        {
            option.AddAppDomain(AppDomain.CurrentDomain);
            option.DisableConcurrentlyJobExecution();
            option.WebRequestJobTimeout(TimeSpan.FromSeconds(15));
        }));

    services.AddHangfireServer();
}
```

## Usage

### Dashboard Usage

1. **Run the Example Project**: Start the project and navigate to `http://localhost:<port>/hangfire` to access the Hangfire Dashboard.

2. **Add a New Job**: Go to the Recurring Job Manager section in the dashboard. Add a new job, selecting either the `MethodCall` or `WebRequestJob` type. Enter the necessary parameters, such as HTTP headers, body, and query parameters, directly through the dashboard interface.

3. **Manage Jobs**: View, edit, delete, pause, or restart the jobs from the dashboard as needed.

4. **Fire-and-Forget Jobs via API**: WebRequestJobs can now be added externally as fire-and-forget jobs via the provided API endpoint `/api/web-request-job`.

5. **Automatic Resolution**: Methods and types within the project are automatically detected, allowing you to configure their parameters directly via the Hangfire Dashboard.

### Client Usage (Programmatic)

To simplify adding web request jobs programmatically, use the `HangfireWebRequestJobApiClient`:

```csharp
// Instantiate client
var client = new HangfireWebRequestJobApiClient(new Uri("https://your-api-host.com"));
client.Headers.Add(new HttpHeaderParameter { Name = "Authorization", Value = "Bearer <token>" });
```

#### JSON Body Example

```csharp
var jobJson = new WebRequestJobBodyJson
{
    Uri = new Uri("https://api.example.com/v1/customers"),
    Method = HttpMethodType.POST,
    HeaderParameters = new []
    {
        new HttpHeaderParameter { Name = "Content-Type", Value = "application/json" }
    },
    BodyParameters = new { name = "Jane Doe", email = "jane@example.com" }
};

var response = await client.AddAsync(jobJson);
Console.WriteLine(response.StatusCode);
```

#### XML Body Example

```csharp
var xmlDoc = new XmlDocument();
xmlDoc.LoadXml("<Customer><Name>Jane</Name><Email>jane@example.com</Email></Customer>");

var jobXml = new WebRequestJobBodyXml
{
    Uri = new Uri("https://api.example.com/v1/customers"),
    Method = HttpMethodType.POST,
    HeaderParameters = new []
    {
        new HttpHeaderParameter { Name = "Content-Type", Value = "application/xml" }
    },
    BodyParameters = xmlDoc
};

var response = await client.AddAsync(jobXml);
```

#### FormUrlEncoded Example

```csharp
var jobFormUrl = new WebRequestJobBodyFormUrlEncoded
{
    Uri = new Uri("https://api.example.com/v1/login"),
    Method = HttpMethodType.POST,
    HeaderParameters = new []
    {
        new HttpHeaderParameter { Name = "Content-Type", Value = "application/x-www-form-urlencoded" }
    },
    BodyParameters = new[]
    {
        new HttpFormUrlEncodedParameter { Name = "username", Value = "jane" },
        new HttpFormUrlEncodedParameter { Name = "password", Value = "123456" }
    }
};

var response = await client.AddAsync(jobFormUrl);
```

#### FormData Example

```csharp
var jobFormData = new WebRequestJobBodyFormData
{
    Uri = new Uri("https://api.example.com/v1/upload"),
    Method = HttpMethodType.POST,
    HeaderParameters = new []
    {
        new HttpHeaderParameter { Name = "Authorization", Value = "Bearer <token>" }
    },
    BodyParameters = new[]
    {
        new HttpFormDataParameter { Name = "username", Value = "burak.eser", ContentType = "text/plain" },
        new HttpFormDataParameter { Name = "profilePicture", Value = "BASE64_STRING_HERE", ContentType = "image/png" }
    }
};

var response = await client.AddAsync(jobFormData);
```

#### PlainText Body Example

```csharp
var jobText = new WebRequestJobBodyPlainText
{
    Uri = new Uri("https://api.example.com/v1/notes"),
    Method = HttpMethodType.POST,
    HeaderParameters = new []
    {
        new HttpHeaderParameter { Name = "Content-Type", Value = "text/plain" }
    },
    BodyParameters = "This is a plain text note."
};

var response = await client.AddAsync(jobText);
```

#### None Body Example

```csharp
var jobNone = new WebRequestJobBodyNone
{
    Uri = new Uri("https://api.example.com/v1/ping"),
    Method = HttpMethodType.GET,
    HeaderParameters = new []
    {
        new HttpHeaderParameter { Name = "Authorization", Value = "Bearer <token>" }
    }
};

var response = await client.AddAsync(jobNone);
```

> All responses are returned as `WebRequestJobResponse`, containing `StatusCode`, `ExceptionCode`, and `ExceptionMessage`.

## Contributing

To contribute, fork this repository, make your changes, and submit a pull request. For any issues or feedback, please open an issue on GitHub.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.

