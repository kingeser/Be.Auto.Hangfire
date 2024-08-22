#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core
{
    public static class AssemblyInfoStorage
    {
        public static readonly List<AssemblyInfo> Assemblies = [];
        public static void Store(params Assembly[] assemblies)
        {
            Assemblies.AddRange(assemblies.Select(t => new AssemblyInfo(t)));
        }

        public static Type? GetType(RecurringJobMethodCall job)
        {
            return Assemblies.SelectMany(t => t.TypeMethods).Where(t => t.Key.FullName == job.Type).Select(t => t.Key).FirstOrDefault();
        }
        public static Type? GetType(string type)
        {
            return Assemblies.SelectMany(t => t.TypeMethods).Where(t => t.Key.FullName == type).Select(t => t.Key).FirstOrDefault();
        }
        public static bool IsValidType(string type)
        {
            return GetType(type) != null;
        }
        public static bool IsValidType(RecurringJobMethodCall job)
        {
            return GetType(job) != null;
        }
        public static MethodInfo? GetMethod(RecurringJobMethodCall job)
        {
            return Assemblies.SelectMany(t => t.TypeMethods).Where(t => t.Key.FullName == job.Type).SelectMany(t => t.Value).FirstOrDefault(t => t.GenerateFullName() == job.Method);
        }
        public static MethodInfo? GetMethod(string type, string method)
        {
            return Assemblies.SelectMany(t => t.TypeMethods).Where(t => t.Key.FullName == type).SelectMany(t => t.Value).FirstOrDefault(t => t.GenerateFullName() == method);
        }
        public static bool IsValidMethod(string type, string method)
        {
            return GetMethod(type, method) != null;
        }
        public static bool IsValidMethod(RecurringJobMethodCall job)
        {
            return GetMethod(job) != null;
        }

        public static List<MethodInfo> GetMethodsByType(string type)
        {
            return Assemblies.SelectMany(t => t.TypeMethods).Where(t => t.Key.FullName == type).SelectMany(t => t.Value)
                .ToList();
        }

        public static List<Type> GetTypes()
        {
            return Assemblies.SelectMany(t => t.TypeMethods).Select(t => t.Key)
                .ToList();
        }
    }
}
