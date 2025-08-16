# Be.Auto.Hangfire.Dashboard.RecurringJobManager &#x20;

## Overview

**Be.Auto.Hangfire.Dashboard.RecurringJobManager** is a specialized library for managing recurring jobs in Hangfire-based projects. It allows you to configure and manage various types of jobs, including method calls and web requests, directly from the Hangfire Dashboard. This enables all job management tasks to be handled through the dashboard interface without needing to manually add jobs in code.

## Version 1.2.0 Highlights

- **Bug Fixes**: Known issues in previous versions have been resolved.
- **Prevent Concurrent Execution Improvements**:Concurrent execution prevention has been improved on a per-job basis.
- **Web Request Jobs API**: Fire-and-forget WebRequestJobs can now be added externally via a dedicated API.
- **Application-Wide Improvements**: General performance and usability improvements across the system.

## Key Features

- **Automatic Type and Method Detection**: Automatically detects types and methods within the project’s assembly, allowing you to input parameters directly through the Hangfire Dashboard.
- **Dependency Injection (DI) Support**: Types registered with DI are automatically resolved.
- **Comprehensive Job Management**: Supports MethodCall and WebRequestJob types, with full control over job creation, editing, and deletion through the dashboard.
- **HTTP Request Jobs**: Allows detailed configuration of HTTP requests, including headers, body, and query parameters, directly through the dashboard. It also prevents collisions by avoiding the duplication of jobs with identical parameters.
- **Fire-and-Forget API**: External systems can enqueue WebRequest jobs directly as fire-and-forget tasks via the new API endpoint.

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

### Example Usage

The `example` folder in this repository provides a sample project demonstrating how to use the library.

1. **Run the Example Project**: Start the project and navigate to `http://localhost:<port>/hangfire` to access the Hangfire Dashboard.

2. **Add a New Job**: Go to the Recurring Job Manager section in the dashboard. Add a new job, selecting either the `MethodCall` or `WebRequestJob` type. Enter the necessary parameters, such as HTTP headers, body, and query parameters, directly through the dashboard interface.

3. **Manage Jobs**: View, edit, delete, pause, or restart the jobs from the dashboard as needed.

4. **Fire-and-Forget Jobs via API**: WebRequestJobs can now be added externally as fire-and-forget jobs via the provided API endpoint `/api/web-request-job`.

5. **Automatic Resolution**: Methods and types within the project are automatically detected, allowing you to configure their parameters directly via the Hangfire Dashboard.

## Contributing

To contribute, fork this repository, make your changes, and submit a pull request. For any issues or feedback, please open an issue on GitHub.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.

