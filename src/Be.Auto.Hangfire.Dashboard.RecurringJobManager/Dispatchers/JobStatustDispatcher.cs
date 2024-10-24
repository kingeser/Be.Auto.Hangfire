using System;
using System.Collections.Generic;
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
    internal sealed class JobStatustDispatcher : IDashboardDispatcher
    {
        public async Task Dispatch([NotNull] DashboardContext context)
        {
            var response = new Response { Status = true };

            try
            {

                var selectedJobs = context.Request.GetQuery("SelectedJobs");


                if (string.IsNullOrWhiteSpace(selectedJobs))
                {
                    response.Status = false;
                    response.Message = "Job Id is missing or empty.";
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    return;
                }

                var action = context.Request.GetQuery("Action");


                if (string.IsNullOrWhiteSpace(action))
                {
                    response.Status = false;
                    response.Message = "Action is missing or empty.";
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }

              
               var selectedJobsArray = selectedJobs.Split('|').Where(t => !string.IsNullOrEmpty(t)).ToArray();

                var notValidJobIds = selectedJobsArray.Where(t => !RecurringJobAgent.IsValidJobId(t)).ToArray();

                if (notValidJobIds.Length > 0)
                {
                    response.Status = false;
                    response.Message = $"The Job Id {string.Join(",", notValidJobIds)} was not found.";
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
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
                        return;
                }

                context.Response.StatusCode = (int)HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                response.Status = false;
                response.Message = e.GetAllMessages();
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            }
            finally
            {
                await context.Response.WriteAsync(response.SerializeObjectToJson());
            }
        }
    }
}
