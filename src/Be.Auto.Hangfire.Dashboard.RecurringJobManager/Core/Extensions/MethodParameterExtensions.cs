using System;
using System.Collections;
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
            return (method?.GetParameters() ?? []).Select(p => CreateInstanceWithDefaults(p.ParameterType)).ToArray();
        }

        public static object GetParameterNamesAndDefaults(this MethodInfo method)
        {
            var expando = new ExpandoObject() as IDictionary<string, object>;

            foreach (var param in method?.GetParameters() ?? [])
            {
                expando[param.Name] = CreateInstanceWithDefaults(param.ParameterType);
            }

            return expando;
        }

        private static object CreateInstanceWithDefaults(Type type)
        {

            if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
                return null;


            if (Nullable.GetUnderlyingType(type) != null)
                return Activator.CreateInstance(type);

            if (type == typeof(string))
                return string.Empty;


            if (type.IsValueType)
                return Activator.CreateInstance(type);


            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                if (elementType != null)
                {
                    var arrayInstance = Array.CreateInstance(elementType, 1);
                    arrayInstance.SetValue(CreateInstanceWithDefaults(elementType), 0);
                    return arrayInstance;
                }
            }


            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();

                if (genericTypeDefinition == typeof(Dictionary<,>))
                {
                    var keyType = type.GetGenericArguments()[0];
                    var valueType = type.GetGenericArguments()[1];
                    var dictionaryInstance = Activator.CreateInstance(type) as IDictionary;
                    var keyInstance = CreateInstanceWithDefaults(keyType);
                    var valueInstance = CreateInstanceWithDefaults(valueType);
                    if (dictionaryInstance != null)
                    {
                        dictionaryInstance.Add(keyInstance, valueInstance);
                        return dictionaryInstance;
                    }
                }

                if (genericTypeDefinition == typeof(List<>))
                {
                    var elementType = type.GetGenericArguments()[0];
                    var listInstance = Activator.CreateInstance(type) as IList;
                    var elementInstance = CreateInstanceWithDefaults(elementType);
                    if (listInstance != null)
                    {
                        listInstance.Add(elementInstance);
                        return listInstance;
                    }
                }


                if (genericTypeDefinition == typeof(Tuple<,,>))
                {
                    var arguments = type.GetGenericArguments()
                        .Select(CreateInstanceWithDefaults)
                        .ToArray();
                    return Activator.CreateInstance(type, arguments);
                }
            }


            var instance = Activator.CreateInstance(type);


            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite && p.GetIndexParameters().Length == 0))
            {
                var propertyType = property.PropertyType;
                var propertyValue = CreateInstanceWithDefaults(propertyType);
                property.SetValue(instance, propertyValue);
            }

            return instance;
        }
    }



}
