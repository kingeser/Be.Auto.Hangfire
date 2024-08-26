using Hangfire.Annotations;
using Hangfire.Dashboard;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using System;
using System.Threading.Tasks;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;
using System.Net;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dispatchers
{
    internal sealed class GetJobForEdit : IDashboardDispatcher
    {

        public async Task Dispatch([NotNull] DashboardContext conterecurringJobt)
        {
            var response = new Response() { Status = true };

            try
            {
                if (!"GET".Equals(conterecurringJobt.Request.Method, StringComparison.InvariantCultureIgnoreCase))
                {
                    response.Status = false;

                    conterecurringJobt.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    return;
                }

                var jobId = conterecurringJobt.Request.GetQuery("Id");

                var recurringJob = RecurringJobAgent.GetJob(jobId);

                if (recurringJob == null)
                {
                    response.Status = false;

                    response.Message = "Job not found";

                    conterecurringJobt.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                    return;
                }

                conterecurringJobt.Response.StatusCode = (int)HttpStatusCode.OK;

                response.Object = recurringJob;
            }
            catch (Exception e)
            {
                response.Status = false;
                response.Message = e.GetAllMessages();
                conterecurringJobt.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            finally
            {
                await conterecurringJobt.Response.WriteAsync(response.SerializeObjectToJson());
            }


        }
    }
}
