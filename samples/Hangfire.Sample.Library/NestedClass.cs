using System;
using System.Collections.Generic;

namespace Hangfire.Sample.Library
{
    public class NestedClass
    {
        public string Description { get; set; }
        public List<DateTime> Dates { get; set; }
        public AnotherNestedClass AnotherNested { get; set; }
    }
}