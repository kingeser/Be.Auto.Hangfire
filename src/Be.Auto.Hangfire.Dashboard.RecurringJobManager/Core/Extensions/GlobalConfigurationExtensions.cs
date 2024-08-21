using Hangfire.Annotations;
using Hangfire.Dashboard;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Pages;
using System;
using System.Reflection;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dispatchers;
using Hangfire;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions
{
    public static class GlobalConfigurationExtensions
    {

        [PublicAPI]
        public static IGlobalConfiguration UseDashboardRecurringJobManager(this IGlobalConfiguration config,IServiceProvider serviceProvider,[NotNull] params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
            {
                throw new RecurringJobException("The assemblies parameter is either null or an empty array. Please provide at least one assembly to continue.");
            }

            config.UseActivator(new HangfireJobActivator(serviceProvider));
        
            StoreAssemblies(assemblies);
           InitializeDashboard();

            return config;
        }

        private static void StoreAssemblies(Assembly[] assemblies)
        {
            AssemblyInfoStorage.Store(assemblies);
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
        }

        private static void AddApiRoutes()
        {
            DashboardRoutes.Routes.Add("/jobs/get-job-stopped", new GetJobsStoppedDispatcher());
            DashboardRoutes.Routes.Add("/recurring-job-manager/get-jobs", new GetJobDispatcher());
            DashboardRoutes.Routes.Add("/recurring-job-manager/update-jobs", new SaveJobDispatcher());
            DashboardRoutes.Routes.Add("/recurring-job-manager/get-job", new GetJobForEdit());
            DashboardRoutes.Routes.Add("/recurring-job-manager/job-agent", new JobAgentDispatcher());
            DashboardRoutes.Routes.Add("/recurring-job-manager/get-job-types", new GetJobTypesDispatcher());
            DashboardRoutes.Routes.Add("/recurring-job-manager/get-body-parameter-types", new GetBodyTypesDispatcher());
            DashboardRoutes.Routes.Add("/recurring-job-manager/get-current-assembly-types", new GetCurrentAssemblyTypesDispatcher());
            DashboardRoutes.Routes.Add("/recurring-job-manager/get-current-assembly-type-methods", new GetCurrentAssemblyTypeMethodsDispatcher());
            DashboardRoutes.Routes.Add("/recurring-job-manager/get-http-methods", new GetHttpMethodsDispatcher());
            DashboardRoutes.Routes.Add("/recurring-job-manager/get-misfire-handling-modes", new GetMisfireHandlingModesDispatcher());
            DashboardRoutes.Routes.Add("/recurring-job-manager/get-time-zones", new GetTimeZonesDispatcher());
            DashboardRoutes.Routes.Add("/recurring-job-manager/get-current-assembly-type-method-json-schema", new GetCurrentAssemblyTypeMethodJsonSchemaDispatcher());
        }

        private static void AddMetrics()
        {
            DashboardMetrics.AddMetric(TagDashboardMetrics.JobsStoppedCount);
        }

        private static void AddMenuItems()
        {
            JobsSidebarMenu.Items.Add(page => new MenuItem("Jobs Stopped", page.Url.To("/jobs/stopped"))
            {
                Active = page.RequestPath.StartsWith("/jobs/stopped"),
                Metric = TagDashboardMetrics.JobsStoppedCount,
            });

            NavigationMenu.Items.Add(page => new MenuItem(JobExtensionPage.Title, page.Url.To("/recurring-job-manager"))
            {
                Active = page.RequestPath.StartsWith(JobExtensionPage.PageRoute),
                Metric = DashboardMetrics.RecurringJobCount
            });
        }

        private static void AddEmbeddedResources()
        {
            AddDashboardRouteToEmbeddedResource("/recurring-job-manager/webfonts/fa-brands-400.eot", "text/css", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.fonts.fa-brands-400.eot");
            AddDashboardRouteToEmbeddedResource("/recurring-job-manager/webfonts/fa-brands-400.ttf", "text/css", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.fonts.fa-brands-400.ttf");
            AddDashboardRouteToEmbeddedResource("/recurring-job-manager/webfonts/fa-solid-900.ttf", "text/css", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.fonts.fa-solid-900.ttf");
            AddDashboardRouteToEmbeddedResource("/recurring-job-manager/webfonts/fa-solid-900.woff2", "text/css", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.fonts.fa-solid-900.woff2");

            AddDashboardRouteToEmbeddedResource("/recurring-job-manager/css/fontawesome", "text/css", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.css.fontawesome.css");
            AddDashboardRouteToEmbeddedResource("/recurring-job-manager/css/jobExtension", "text/css", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.css.JobExtension.css");
            AddDashboardRouteToEmbeddedResource("/recurring-job-manager/css/cron-expression-input", "text/css", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.css.cron-expression-input.css");
            AddDashboardRouteToEmbeddedResource("/recurring-job-manager/js/page", "application/javascript", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.js.jobextension.js");
            AddDashboardRouteToEmbeddedResource("/recurring-job-manager/js/vue", "application/javascript", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.js.vue.js");
            AddDashboardRouteToEmbeddedResource("/recurring-job-manager/js/axio", "application/javascript", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.js.axios.min.js");
            AddDashboardRouteToEmbeddedResource("/recurring-job-manager/js/daysjs", "application/javascript", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.js.daysjs.min.js");
            AddDashboardRouteToEmbeddedResource("/recurring-job-manager/js/relativeTime", "application/javascript", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.js.relativeTime.min.js");
            AddDashboardRouteToEmbeddedResource("/recurring-job-manager/js/sweetalert", "application/javascript", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.js.sweetalert.js");
            AddDashboardRouteToEmbeddedResource("/recurring-job-manager/js/cron-expression-input", "application/javascript", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.js.cron-expression-input.js");
            AddDashboardRouteToEmbeddedResource("/recurring-job-manager/js/jsoneditor", "application/javascript", "Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dashboard.Content.js.jsoneditor.min.js");
        }

        private static void AddDashboardRouteToEmbeddedResource(string route, string contentType, string resourceName)
        {
            DashboardRoutes.Routes.Add(route, new ContentDispatcher(contentType, resourceName, TimeSpan.FromDays(1)));
        }
    }
}
