using System;
using System.Collections.Generic;
using System.Reflection;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models
{
    public class JobManagerOption
    {
        internal ICollection<Assembly> Assemblies { get; set; } = new List<Assembly>();
        internal ConcurrentJobExecution ConcurrentJobExecution { get; set; } = ConcurrentJobExecution.Allow;
        internal WebRequestJobOption WebRequestJob { get; set; } = new WebRequestJobOption();


        public JobManagerOption WebRequestJobTimeout(TimeSpan timeout)
        {
            WebRequestJob.TimeOut = timeout;
            return this;
        }

        public JobManagerOption DisableConcurrentlyJobExecution()
        {
            this.ConcurrentJobExecution = ConcurrentJobExecution.Disable;
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
