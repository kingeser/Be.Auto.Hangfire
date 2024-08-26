using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hangfire.Sample.Library
{
    public static class StaticJobTest
    {

        public static async Task<string> Test(string abc, int count)
        {
            Thread.Sleep(5000);
            return await Task.FromResult($"{abc} > {count}");
        }
    }
}