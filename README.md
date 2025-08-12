
# BKB.Be.Auto.Hangfire.Dashboard.RecurringJobManager

This is a fork of [Be.Auto.Hangfire.Dashboard.RecurringJobManager](https://github.com/kingeser/Be.Auto.Hangfire) with additional bug fixes and enhancements.

## Why This Fork?
- Original maintainer appears inactive
- Critical bugs needed fixing

## Changes from Original
- Bug fixes for job management

## Migration from Original
```csharp
// Old
Install-Package Be.Auto.Hangfire.Dashboard.RecurringJobManager
// New  
Install-Package BKB.Be.Auto.Hangfire.Dashboard.RecurringJobManager
```

# Be.Auto.Hangfire.Dashboard.RecurringJobManager

![image](https://github.com/user-attachments/assets/96cc0f8a-e509-4108-9f14-1d41e1ad2925)

![image](https://github.com/user-attachments/assets/6d5c97ba-7149-4373-92a0-29bb3b01b271)

## Overview


**Be.Auto.Hangfire.Dashboard.RecurringJobManager** is a specialized library for managing recurring jobs in Hangfire-based projects. It allows you to configure and manage various types of jobs, including method calls and web requests, directly from the Hangfire Dashboard. This enables all job management tasks to be handled through the dashboard interface without needing to manually add jobs in code.


## Key Features

- **Automatic Type and Method Detection**: Automatically detects types and methods within the project’s assembly, allowing you to input parameters directly through the Hangfire Dashboard.
- **Dependency Injection (DI) Support**: Types registered with DI are automatically resolved.
- **Comprehensive Job Management**: Supports MethodCall and WebRequestJob types, with full control over job creation, editing, and deletion through the dashboard.
- **HTTP Request Jobs**: Allows detailed configuration of HTTP requests, including headers, body, and query parameters, directly through the dashboard. It also prevents collisions by avoiding the duplication of jobs with identical parameters.

## Installation

1. **Add the NuGet Package**:
   Install the `Be.Auto.Hangfire.Dashboard.RecurringJobManager` package to your project:
   ```bash
   Install-Package Be.Auto.Hangfire.Dashboard.RecurringJobManager
   ```

2. **Configure Services**:
   In your `Startup.cs`, add the following configuration:
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

The `example` folder in this repository provides a sample project that demonstrates how to use the `Be.Auto.Hangfire.Dashboard.RecurringJobManager` library. Here are the steps to run the example and manage jobs:

1. **Run the Example Project**:
   - Start the project and navigate to `http://localhost:<port>/hangfire` to access the Hangfire Dashboard.

2. **Add a New Job**:
   - Go to the `Recurring Job Manager` section in the dashboard. Add a new job, selecting either the MethodCall or WebRequestJob type. Enter the necessary parameters, such as HTTP headers, body, and query parameters, directly through the dashboard interface.

3. **Manage Jobs**:
   - View, edit, delete, pause, or restart the jobs from the dashboard as needed.

4. **Automatic Resolution**:
   - Methods and types within the project are automatically detected, allowing you to configure their parameters directly via the Hangfire Dashboard.

## Contributing

To contribute, fork this repository, make your changes, and submit a pull request. For any issues or feedback, please open an issue on GitHub.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.
