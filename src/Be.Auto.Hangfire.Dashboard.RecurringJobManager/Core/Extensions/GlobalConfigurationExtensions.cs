using Hangfire.Annotations;
using Hangfire.Dashboard;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Pages;
using System;
using System.Linq;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Attributes;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dispatchers;
using Hangfire;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions
{


    public static class GlobalConfigurationExtensions
    {

        [PublicAPI]
        public static IGlobalConfiguration UseDashboardRecurringJobManager(this IGlobalConfiguration config, Action<JobManagerOption> options)
        {

            options.Invoke(Options.Instance);

            config.UseFilter(new DisableConcurrentlyJobExecutionAttribute());

            config.UseFilter(new StateHandlerFilterAttribute(new CancelledStateHandler()));

            StoreAssemblies();

            InitializeDashboard();

            return config;
        }

        private static void StoreAssemblies()
        {
            if (Options.Instance.Assemblies != null && Options.Instance.Assemblies.Any())
                AssemblyInfoStorage.Store(Options.Instance.Assemblies);
        }

        private static void InitializeDashboard()
        {
            AddRazorPages();
            AddApiRoutes();
            AddMetrics();
            AddMenuItems();
            AddEmbeddedResources();
        }

        private static void AddRazorPages()
        {
            DashboardRoutes.Routes.AddRazorPage(JobExtensionPage.PageRoute, x => new JobExtensionPage());
            DashboardRoutes.Routes.AddRazorPage(JobsStoppedPage.PageRoute, x => new JobsStoppedPage());
            DashboardRoutes.Routes.AddRazorPage(JobsCancelledPage.PageRoute, x => new JobsCancelledPage());

        }

        private static void AddApiRoutes()
        {
            DashboardRoutes.Routes.Add("/api/web-request-job",new WebRequestJobSaveDispatcher());
            DashboardRoutes.Routes.Add("/jobs/get-job-cancelled", new GetJobsCancelledDispatcher());
            DashboardRoutes.Routes.Add("/jobs/get-job-stopped", new GetJobsStoppedDispatcher());
            DashboardRoutes.Routes.Add("/job-manager/get-jobs", new GetJobDispatcher());
            DashboardRoutes.Routes.Add("/job-manager/update-jobs", new SaveJobDispatcher());
            DashboardRoutes.Routes.Add("/job-manager/delete-job", new JobDeleteDispatcher());
            DashboardRoutes.Routes.Add("/job-manager/get-job", new GetJobForEdit());
            DashboardRoutes.Routes.Add("/job-manager/job-agent", new JobStatustDispatcher());
            DashboardRoutes.Routes.Add("/job-manager/job-export-import", new JobExportImportDispatcher());
            DashboardRoutes.Routes.Add("/job-manager/get-job-types", new GetJobTypesDispatcher());
            DashboardRoutes.Routes.Add("/job-manager/get-body-parameter-types", new GetBodyTypesDispatcher());
            DashboardRoutes.Routes.Add("/job-manager/get-current-assembly-types", new GetCurrentAssemblyTypesDispatcher());
            DashboardRoutes.Routes.Add("/job-manager/get-current-assembly-type-methods", new GetCurrentAssemblyTypeMethodsDispatcher());
            DashboardRoutes.Routes.Add("/job-manager/get-http-methods", new GetHttpMethodsDispatcher());
            DashboardRoutes.Routes.Add("/job-manager/get-misfire-handling-modes", new GetMisfireHandlingModesDispatcher());
            DashboardRoutes.Routes.Add("/job-manager/get-time-zones", new GetTimeZonesDispatcher());
            DashboardRoutes.Routes.Add("/job-manager/get-current-assembly-type-method-json-schema", new GetCurrentAssemblyTypeMethodJsonSchemaDispatcher());
        }

        private static void AddMetrics()
        {
            DashboardMetrics.AddMetric(TagDashboardMetrics.JobsStoppedCount);
            DashboardMetrics.AddMetric(TagDashboardMetrics.JobsCancelledCount);
        }

        private static void AddMenuItems()
        {
            JobsSidebarMenu.Items.Add(page => new MenuItem("Stopped", page.Url.To("/jobs/stopped"))
            {
                Active = page.RequestPath.StartsWith("/jobs/stopped"),
                Metric = TagDashboardMetrics.JobsStoppedCount,
            });

            JobsSidebarMenu.Items.Add(page => new MenuItem("Cancelled", page.Url.To("/jobs/cancelled"))
            {
                Active = page.RequestPath.StartsWith("/jobs/cancelled"),
                Metric = TagDashboardMetrics.JobsCancelledCount,
            });

            NavigationMenu.Items.Add(page => new MenuItem(JobExtensionPage.Title, page.Url.To("/job-manager"))
            {
                Active = page.RequestPath.StartsWith(JobExtensionPage.PageRoute),
                Metric = DashboardMetrics.RecurringJobCount
            });


            NavigationMenu.Items.Add(page => new MenuItem("API Documentation", page.Url.To("/api/web-request-job"))
            {
                Active = page.RequestPath.StartsWith("/api/web-request-job"),
               
            });
        }

        private static void AddEmbeddedResources()
        {
            AddDashboardRouteToEmbeddedResource("/job-manager/webfonts/fa-brands-400.eot", "text/css", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.fonts.fa-brands-400.eot");
            AddDashboardRouteToEmbeddedResource("/job-manager/webfonts/fa-brands-400.ttf", "text/css", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.fonts.fa-brands-400.ttf");
            AddDashboardRouteToEmbeddedResource("/job-manager/webfonts/fa-solid-900.ttf", "text/css", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.fonts.fa-solid-900.ttf");
            AddDashboardRouteToEmbeddedResource("/job-manager/webfonts/fa-solid-900.woff2", "text/css", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.fonts.fa-solid-900.woff2");


            AddDashboardRouteToEmbeddedResource("/job-manager/css/fontawesome", "text/css", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.css.fontawesome.css");
            AddDashboardRouteToEmbeddedResource("/job-manager/css/app", "text/css", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.css.app.css");
            AddDashboardRouteToEmbeddedResource("/job-manager/css/cron-expression-input", "text/css", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.css.cron-expression-input.css");



            AddDashboardRouteToEmbeddedResource("/job-manager/js/vue", "application/javascript", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.js.vue.js");
            AddDashboardRouteToEmbeddedResource("/job-manager/js/axio", "application/javascript", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.js.axios.min.js");
            AddDashboardRouteToEmbeddedResource("/job-manager/js/sweetalert", "application/javascript", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.js.sweetalert.js");
            AddDashboardRouteToEmbeddedResource("/job-manager/js/cron-expression-input", "application/javascript", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.js.cron-expression-input.js");
            AddDashboardRouteToEmbeddedResource("/job-manager/js/jsoneditor", "application/javascript", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.js.jsoneditor.min.js");
            AddDashboardRouteToEmbeddedResource("/job-manager/js/relativetime", "application/javascript", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.js.relativetime.js");

        }

        private static void AddDashboardRouteToEmbeddedResource(string route, string contentType, string resourceName)
        {
            DashboardRoutes.Routes.Add(route, new ContentDispatcher(contentType, resourceName, TimeSpan.FromDays(1)));
        }
    }
}
