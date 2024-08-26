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
        public ICollection<Assembly> Assemblies { get; set; } = new List<Assembly>();
        public ConcurrentJobExecution ConcurrentJobExecution { get; set; } = ConcurrentJobExecution.Allow;
        public WebRequestJobOption WebRequestJob { get; set; } = new WebRequestJobOption();

        public void AddAssembly(Assembly assembly)
        {
            Assemblies.Add(assembly);
        }
        public void AddAssembly(params Assembly[] assembly)
        {
            foreach (var item in assembly)
            {
                Assemblies.Add(item);
            }

        }
        public void AddAppDomain(AppDomain domain)
        {
            foreach (var assembly in domain.GetAssemblies())
            {
                Assemblies.Add(assembly);
            }

        }

    }
    public class WebRequestJobOption
    {
        public TimeSpan TimeOut { get; set; } = TimeSpan.FromSeconds(30);

    }

}
