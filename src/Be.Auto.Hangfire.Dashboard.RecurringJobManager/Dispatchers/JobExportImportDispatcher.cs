using System;
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
    internal sealed class JobExportImportDispatcher : IDashboardDispatcher
    {
        public async Task Dispatch([NotNull] DashboardContext context)
        {
            var response = new Response { Status = true };

            try
            {
                var selectedJobs = string.Empty;
                var action = string.Empty;

                if (context.Request.Method == "GET")
                {
                    selectedJobs = context.Request.GetQuery("SelectedJobs");
                    action = context.Request.GetQuery("Action");
                }
                if (context.Request.Method == "POST")
                {
                    selectedJobs = (await context.Request.GetFormValuesAsync("SelectedJobs")).FirstOrDefault() ?? "[]";
                    action = (await context.Request.GetFormValuesAsync("Action")).FirstOrDefault();
                }

                if (string.IsNullOrWhiteSpace(selectedJobs))
                {
                    response.Status = false;
                    response.Message = "Job Id is missing or empty.";
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    return;
                }

                if (string.IsNullOrWhiteSpace(action))
                {
                    response.Status = false;
                    response.Message = "Action is missing or empty.";
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }

                var selectedJobsArray = new string[] { };

                if (!action.Equals("Import", StringComparison.OrdinalIgnoreCase))

                {
                    selectedJobsArray = selectedJobs.Split('|').Where(t => !string.IsNullOrEmpty(t)).ToArray();

                    var notValidJobIds = selectedJobsArray.Where(t => !RecurringJobAgent.IsValidJobId(t)).ToArray();

                    if (notValidJobIds.Length > 0)
                    {
                        response.Status = false;
                        response.Message = $"The Job Id {string.Join(",", notValidJobIds)} was not found.";
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return;
                    }
                }

                switch (action)
                {

                    case "Export":
                        {

                            var jobs = RecurringJobAgent.GetAllJobs().Where(t => selectedJobsArray.Contains(t.Id));

                            var json = jobs.SerializeObjectToJson();

                            response.Message = json;

                        }
                        break;
                    case "Import":
                        {

                        
                            var jobs = selectedJobs.TryDeserializeJobs(out var result);

                            if (!result)
                            {

                                response.Status = false;
                                response.Message = "Wrong json file!";
                            }

                            else
                            {

                                foreach (var recurringJobBase in jobs)
                                {
                                    recurringJobBase.Register();
                                }
                            }

                        }
                        break;
                    default:
                        {
                            response.Status = false;
                            response.Message = $"Action '{action}' is not recognized. Valid actions are 'Start' and 'Stop'.";
                            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            return;
                        }

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
