namespace Hangfire.Sample.Library
{
    public static class StaticJobTest
    {

        public static string Test(string abc, int count)
        {
            return $"{abc} > {count}";
        }
    }
}