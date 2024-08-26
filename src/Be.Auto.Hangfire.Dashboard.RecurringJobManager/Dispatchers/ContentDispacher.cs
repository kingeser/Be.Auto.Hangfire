using Hangfire.Dashboard;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dispatchers
{
    internal sealed class ContentDispatcher(string contentType, string resourceName, TimeSpan expiresIn)
        : IDashboardDispatcher
    {
        private static readonly Assembly ThisAssembly = typeof(ContentDispatcher).Assembly;

        public async Task Dispatch(DashboardContext context)
        {
            context.Response.ContentType = contentType;
            context.Response.SetExpire(DateTimeOffset.UtcNow + expiresIn);
            await WriteResourceAsync(context);
        }

        private async Task WriteResourceAsync(DashboardContext context)
        {
            using var stream = ThisAssembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                context.Response.StatusCode = 404;
            }
            else
            {
                await stream.CopyToAsync(context.Response.Body);
            }
        }
    }
}
