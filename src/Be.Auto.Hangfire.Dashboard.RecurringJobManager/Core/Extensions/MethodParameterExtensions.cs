using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions
{
    public static class MethodParameterExtensions
    {
        public static object[] GetDefaultParameters(this MethodInfo method)
        {
            return method.GetParameters().Select(p => GetDefault(p.ParameterType)).ToArray();
        }

        public static object GetParameterNamesAndDefaults(this MethodInfo method)
        {
            var expando = new ExpandoObject() as IDictionary<string, object>;

            foreach (var param in method.GetParameters())
            {
                expando[param.Name] = GetDefault(param.ParameterType);
            }

            return expando;
        }

        private static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {

                return Activator.CreateInstance(type);
            }

            if (Nullable.GetUnderlyingType(type) != null)
            {

                return null;
            }

            if (type.IsAbstract || type.IsInterface)
            {

                return null;
            }


            if (type == typeof(void))
            {

                return null;
            }


            if (!type.IsClass)
            {

                return null;
            }

            try
            {

                var ctor = type.GetConstructors()
                               .OrderBy(c => c.GetParameters().Length)
                               .FirstOrDefault();

                if (ctor == null || ctor.GetParameters().Length == 0)
                {

                    var instance = Activator.CreateInstance(type);
                    PopulateProperties(instance);
                    return instance;
                }


                var ctorParams = ctor.GetParameters()
                                     .Select(p => GetDefault(p.ParameterType))
                                     .ToArray();


                var obj = Activator.CreateInstance(type, ctorParams);


                PopulateProperties(obj);

                return obj;
            }
            catch
            {

                return null;
            }
        }

        private static void PopulateProperties(object obj)
        {
            if (obj == null) return;

            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && !p.PropertyType.IsPrimitive && p.PropertyType != typeof(string));

            foreach (var property in properties)
            {
                var value = GetDefault(property.PropertyType);
                property.SetValue(obj, value);


                PopulateProperties(value);
            }
        }
    }
}
