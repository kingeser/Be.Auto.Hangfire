using Hangfire.Annotations;
using Hangfire.Dashboard;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using System.Net;
using System.Threading.Tasks;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dispatchers
{
    public sealed class JobAgentDispatcher : IDashboardDispatcher
    {
        public async Task Dispatch([NotNull] DashboardContext context)
        {
            var response = new Response { Status = true };

            var jobId = context.Request.GetQuery("Id");
            var action = context.Request.GetQuery("Action");

            if (string.IsNullOrWhiteSpace(jobId))
            {
                response.Status = false;
                response.Message = "Job Id is missing or empty.";
                await context.Response.WriteAsync(response.SerializeObjectToJson());
                return;
            }

            if (string.IsNullOrWhiteSpace(action))
            {
                response.Status = false;
                response.Message = "Action is missing or empty.";
                await context.Response.WriteAsync(response.SerializeObjectToJson());
                return;
            }

            if (!RecurringJobAgent.IsValidJobId(jobId))
            {
                response.Status = false;
                response.Message = $"The Job Id {jobId} was not found.";
                await context.Response.WriteAsync(response.SerializeObjectToJson());
                return;
            }

            switch (action)
            {
                case "Stop":
                    RecurringJobAgent.StopBackgroundJob(jobId);
                    break;
                case "Start":
                    RecurringJobAgent.StartBackgroundJob(jobId);
                    break;
                default:
                    response.Status = false;
                    response.Message = $"Action '{action}' is not recognized. Valid actions are 'Start' and 'Stop'.";
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await context.Response.WriteAsync(response.SerializeObjectToJson());
                    return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.OK;

            await context.Response.WriteAsync(response.SerializeObjectToJson());
        }
    }
}
