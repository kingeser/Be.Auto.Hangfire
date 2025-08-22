namespace Sample.Test
{

  

    public class TestClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public NestedClass Nested { get; set; }
        public Dictionary<string, string> Details { get; set; }

        public string[] Options { get; set; }
    }

    public class LongDuringJobTest
    {


        public async Task LongTest()
        {

            await Task.Delay(TimeSpan.FromSeconds(10));
        }

        public Task LongTestWithParams(params string[] args)
        {

            return Task.Delay(TimeSpan.FromSeconds(10));
        }
    }
}