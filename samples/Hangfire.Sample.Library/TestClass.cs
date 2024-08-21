using System.Collections.Generic;

namespace Hangfire.Sample.Library
{
    public class TestClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public NestedClass Nested { get; set; }
        public Dictionary<string, string> Details { get; set; }

        public string[] Options { get; set; }
    }
}