using System.Linq;
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

            var selectedJobs = context.Request.GetQuery("SelectedJobs");
            var action = context.Request.GetQuery("Action");

            if (string.IsNullOrWhiteSpace(selectedJobs))
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

            var selectedJobsArray = selectedJobs.Split('|').Where(t => !string.IsNullOrEmpty(t)).ToArray();


            var notValidJobIds = selectedJobsArray.Where(t => !RecurringJobAgent.IsValidJobId(t)).ToArray();

            if (notValidJobIds.Length > 0)
            {
                response.Status = false;
                response.Message = $"The Job Id {string.Join(",", notValidJobIds)} was not found.";
                await context.Response.WriteAsync(response.SerializeObjectToJson());
                return;
            }

            switch (action)
            {
                case "Stop":
                    RecurringJobAgent.StopBackgroundJob(selectedJobsArray);
                    break;
                case "Start":
                    RecurringJobAgent.StartBackgroundJob(selectedJobsArray);
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
