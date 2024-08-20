using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hangfire.Sample.Library
{
    public class AppointmentSmsNotificationJob
    {


        public async Task RunDelayJob(string delayTime)
        {
            var id = Guid.NewGuid();
            Console.WriteLine(id);
            var source = new CancellationTokenSource();
            await Task.Delay(5000, source.Token);
        }

        public async Task RunDelayJob2(string delayTime, bool start, int count)
        {
            var id = Guid.NewGuid();
            Console.WriteLine(id);
            var source = new CancellationTokenSource();
            await Task.Delay(5000, source.Token);
        }

        public async Task RunDelayJob3(TestClass test,string delayTime, DateTime next)
        {
            var id = Guid.NewGuid();
            Console.WriteLine(id);
            var source = new CancellationTokenSource();
            await Task.Delay(5000, source.Token);
        }



        public class TestClass
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public NestedClass Nested { get; set; }
            public Dictionary<string, string> Details { get; set; }

            public string[] Options { get; set; }
        }

        public class NestedClass
        {
            public string Description { get; set; }
            public List<DateTime> Dates { get; set; }
            public AnotherNestedClass AnotherNested { get; set; }
        }

        public class AnotherNestedClass
        {
            public DateTime Date { get; set; }
            public Tuple<int, string, object> Details { get; set; }
        }
    }
}
