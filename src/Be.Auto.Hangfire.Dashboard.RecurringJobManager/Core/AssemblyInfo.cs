using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core
{
    public class AssemblyInfo(Assembly assembly)
    {
        private Dictionary<Type, List<MethodInfo>> _types;

        public Assembly Assembly { get; set; } = assembly ?? throw new ArgumentNullException(nameof(assembly));

        public Dictionary<Type, List<MethodInfo>> TypeMethods => _types ??= GetTypes(Assembly);

        private static Dictionary<Type, List<MethodInfo>> GetTypes(Assembly assembly)
        {
            if (assembly == null)
                return new Dictionary<Type, List<MethodInfo>>();

            var result = assembly
                .GetTypes()
               
                .Where(type => (type.IsClass && !type.IsInterface) && (type.IsAbstract && type.IsSealed || !type.IsAbstract))
                .Select(type => new
                {
                    Type = type,

                    Methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                        .Where(m => !m.IsSpecialName)
                        .ToList()
                })
                .Where(typeWithMethods => typeWithMethods.Methods.Any())
                .ToDictionary(typeWithMethods => typeWithMethods.Type, typeWithMethods => typeWithMethods.Methods);

            return result;
        }
    }
}