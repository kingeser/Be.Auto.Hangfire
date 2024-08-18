using Hangfire.Annotations;
using Hangfire.Dashboard;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Hangfire.Storage;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;
using Hangfire;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Dispatchers
{
    internal sealed class GetJobForEdit : IDashboardDispatcher
    {
      
        public async Task Dispatch([NotNull] DashboardContext conterecurringJobt)
        {
            var response = new Response() { Status = true };

            if (!"GET".Equals(conterecurringJobt.Request.Method, StringComparison.InvariantCultureIgnoreCase))
            {
                conterecurringJobt.Response.StatusCode = 405;

                return;
            }

            var jobId = conterecurringJobt.Request.GetQuery("Id");

            var recurringJob = RecurringJobAgent.GetJob(jobId);

            if (recurringJob == null)
            {
                response.Status = false;
                response.Message = "Job not found";

                await conterecurringJobt.Response.WriteAsync(response.SerializeObjectToJson());

                return;
            }

            response.Object = recurringJob;

            await conterecurringJobt.Response.WriteAsync(response.SerializeObjectToJson());
        }
    }
}
