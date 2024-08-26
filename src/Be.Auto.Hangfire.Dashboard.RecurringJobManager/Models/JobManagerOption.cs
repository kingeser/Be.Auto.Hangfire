using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models
{
    public enum ConcurrentJobExecution
    {
        Allow,
        Disable
    }
    public class JobManagerOption
    {
        internal ICollection<Assembly> Assemblies { get; set; } = new List<Assembly>();
        internal ConcurrentJobExecution ConcurrentJobExecution { get; set; } = ConcurrentJobExecution.Allow;
        internal WebRequestJobOption WebRequestJob { get; set; } = new WebRequestJobOption();


        public JobManagerOption SetWebRequestJobTimeout(TimeSpan timeout)
        {
            WebRequestJob.TimeOut = timeout;
            return this;
        }

        public JobManagerOption SetConcurrentJobExecution(ConcurrentJobExecution execution)
        {
            this.ConcurrentJobExecution = execution;
            return this;
        }

        public JobManagerOption AddAssembly(Assembly assembly)
        {
            Assemblies.Add(assembly);
            return this;
        }
        public JobManagerOption AddAssembly(params Assembly[] assembly)
        {
            foreach (var item in assembly)
            {
                Assemblies.Add(item);
            }
            return this;
        }
        public JobManagerOption AddAppDomain(AppDomain domain)
        {
            foreach (var assembly in domain.GetAssemblies())
            {
                Assemblies.Add(assembly);
            }
            return this;
        }

    }
}
