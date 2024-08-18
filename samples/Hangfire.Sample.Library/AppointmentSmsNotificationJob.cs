using System;
using System.Threading;
using System.Threading.Tasks;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Attributes;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;

namespace Hangfire.Sample.Library
{
    public class AppointmentSmsNotificationJob
    {
       
        [RecurringJob("*/1 * * * *", "default", RecurringJobId = "RunDelayJob",Enabled = true)]
       public async Task RunDelayJob(string delayTime)
        {
            var id = Guid.NewGuid();
            Console.WriteLine(id);
            var source = new CancellationTokenSource();
            await Task.Delay(180000, source.Token);
        }
        [RecurringJob("*/1 * * * *", "default", RecurringJobId = "RunDelayJob2",Enabled = true)]
        public async Task RunDelayJob2(string delayTime,bool start,int count)
        {
            var id = Guid.NewGuid();
            Console.WriteLine(id);
            var source = new CancellationTokenSource();
            await Task.Delay(180000, source.Token);
        }
        [RecurringJob("*/1 * * * *", "default", RecurringJobId = "RunDelayJob3",Enabled = true)]
        public async Task RunDelayJob3(string delayTime,DateTime next)
        {
            var id = Guid.NewGuid();
            Console.WriteLine(id);
            var source = new CancellationTokenSource();
            await Task.Delay(180000, source.Token);
        }

    }
}
