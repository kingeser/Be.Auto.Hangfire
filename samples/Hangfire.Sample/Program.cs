using System;
using System.Drawing;
using System.Security.Claims;
using System.Threading.Tasks;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sample.Options;

namespace Sample;

public static class Program
{
    public static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        var hangfireOptions = GetHangfireOptions(builder);

        ConfigureHangfire(builder, hangfireOptions);

        var app = builder.Build();

        ConfigureMiddleware(app, hangfireOptions);

        app.Run();


    }


    private static void ConfigureHangfire(WebApplicationBuilder builder, HangfireOptions hangfireOptions)
    {
        builder.Services.AddHangfire(config => config
            .UseSqlServerStorage(hangfireOptions.SqlServerStorageOptions.ConnectionString, new SqlServerStorageOptions()
            {
                QueuePollInterval = hangfireOptions.SqlServerStorageOptions.QueuePollInterval,
                SlidingInvisibilityTimeout = hangfireOptions.SqlServerStorageOptions.SlidingInvisibilityTimeout,
                JobExpirationCheckInterval = hangfireOptions.SqlServerStorageOptions.JobExpirationCheckInterval,
                CountersAggregateInterval = hangfireOptions.SqlServerStorageOptions.CountersAggregateInterval,
                DashboardJobListLimit = hangfireOptions.SqlServerStorageOptions.DashboardJobListLimit,
                TransactionTimeout = hangfireOptions.SqlServerStorageOptions.TransactionTimeout,
                DisableGlobalLocks = hangfireOptions.SqlServerStorageOptions.DisableGlobalLocks,
                DeleteExpiredBatchSize = hangfireOptions.SqlServerStorageOptions.DeleteExpiredBatchSize,
                UseTransactionalAcknowledge = hangfireOptions.SqlServerStorageOptions.UseTransactionalAcknowledge,
                UseRecommendedIsolationLevel = hangfireOptions.SqlServerStorageOptions.UseRecommendedIsolationLevel,
                CommandBatchMaxTimeout = hangfireOptions.SqlServerStorageOptions.CommandBatchMaxTimeout,
                InactiveStateExpirationTimeout = hangfireOptions.SqlServerStorageOptions.InactiveStateExpirationTimeout,
                CommandTimeout = hangfireOptions.SqlServerStorageOptions.CommandTimeout,
                EnableHeavyMigrations = hangfireOptions.SqlServerStorageOptions.EnableHeavyMigrations,
                SchemaName = hangfireOptions.SqlServerStorageOptions.SchemaName,
                TryAutoDetectSchemaDependentOptions = hangfireOptions.SqlServerStorageOptions.TryAutoDetectSchemaDependentOptions,
                UseFineGrainedLocks = hangfireOptions.SqlServerStorageOptions.UseFineGrainedLocks,
                PrepareSchemaIfNecessary = hangfireOptions.SqlServerStorageOptions.PrepareSchemaIfNecessary,
                UseIgnoreDupKeyOption = hangfireOptions.SqlServerStorageOptions.UseIgnoreDupKeyOption,
                SqlClientFactory = SqlClientFactory.Instance,
                

            })
            .UseFilter(new AutomaticRetryAttribute()
            {
                OnAttemptsExceeded = hangfireOptions.JobAutomaticRetryOptions.OnAttemptsExceeded,
                Attempts = hangfireOptions.JobAutomaticRetryOptions.Attempts,
                LogEvents = hangfireOptions.JobAutomaticRetryOptions.LogEvents,
                
            })
           .UseDashboardRecurringJobManager(option =>
            {
                option.AddAppDomain(AppDomain.CurrentDomain);

                if (hangfireOptions.JobManagerOptions.DisableConcurrentlyJobExecution)
                {
                    option.DisableConcurrentlyJobExecution();
                }

                option.WebRequestJobTimeout(hangfireOptions.JobManagerOptions.WebRequestJobTimeout);

            }));



        builder.Services.AddHangfireServer(options =>
        {
            options.ServerName = hangfireOptions.ServerOptions.ServerName;
            options.IsLightweightServer = hangfireOptions.ServerOptions.IsLightweightServer;
            options.WorkerCount = hangfireOptions.ServerOptions.WorkerCount;
            options.Queues = hangfireOptions.ServerOptions.Queues;
            options.StopTimeout = hangfireOptions.ServerOptions.StopTimeout;
            options.ShutdownTimeout = hangfireOptions.ServerOptions.ShutdownTimeout;
            options.SchedulePollingInterval = hangfireOptions.ServerOptions.SchedulePollingInterval;
            options.HeartbeatInterval = hangfireOptions.ServerOptions.HeartbeatInterval;
            options.ServerCheckInterval = hangfireOptions.ServerOptions.ServerCheckInterval;
            options.ServerTimeout = hangfireOptions.ServerOptions.ServerTimeout;
            options.CancellationCheckInterval = hangfireOptions.ServerOptions.CancellationCheckInterval;
            options.MaxDegreeOfParallelismForSchedulers = hangfireOptions.ServerOptions.MaxDegreeOfParallelismForSchedulers;
            options.TimeZoneResolver = new DefaultTimeZoneResolver();
            options.Activator=JobActivator.Current;
            options.TaskScheduler=TaskScheduler.Current;
          
        });

        builder.Services.AddHttpContextAccessor();
    }

    
   

    private static void ConfigureMiddleware(WebApplication app, HangfireOptions hangfireOptions)
    {
        app.UseHttpsRedirection();

        app.UseRouting();
    
        app.UseHangfireDashboard("", new DashboardOptions
        {
            DarkModeEnabled = false,
            DashboardTitle = hangfireOptions.DashboardOptions.DashboardTitle,
            AppPath = "/",
            TimeZoneResolver = new DefaultTimeZoneResolver(),
            DisplayNameFunc = DisplayNameFunctions.WithJobId,
            DisplayStorageConnectionString = hangfireOptions.DashboardOptions.DisplayStorageConnectionString,
            DefaultRecordsPerPage = hangfireOptions.DashboardOptions.DefaultRecordsPerPage,

        });

        app.MapHangfireDashboard("");

     
    }

    private static HangfireOptions GetHangfireOptions(WebApplicationBuilder builder)
    {
        var hangfireDashboardOptions = builder.Configuration.GetSection("Hangfire").Get<HangfireOptions>();
        return hangfireDashboardOptions ?? throw new ArgumentException("Hangfire configuration is not defined in 'appsettings.json'. Please ensure the 'Hangfire' section is correctly set up.");
    }

}
