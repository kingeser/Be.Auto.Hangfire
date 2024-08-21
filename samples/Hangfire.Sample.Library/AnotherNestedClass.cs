using System;

namespace Hangfire.Sample.Library
{
    public class AnotherNestedClass
    {
        public DateTime Date { get; set; }
        public Tuple<int, string, object> Details { get; set; }
    }
}