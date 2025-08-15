using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Newtonsoft.Json;
using NJsonSchema;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions
{
    internal static class MethodInfoExtensions
    {
        public static string GenerateFullName(this MethodInfo @this)
        {
            return $"{@this.Name}({string.Join(",", @this.GetParameters().Select(x => $"{x.ParameterType.Name} {x.Name}"))})";

        }

        public static object[] GetDefaultParameters(this MethodInfo @this, RecurringJobMethodCall job)
        {
            if (string.IsNullOrEmpty(job.MethodParameters))
                return [];

            var parameters = (object[])JsonConvert.DeserializeObject(
                job.MethodParameters,
                typeof(object[]),
                new MethodParameterConverter(@this)
            );

            return parameters ?? [];
        }
        public static object[] GetDefaultParameters(this MethodInfo method)
        {
            return (method?.GetParameters() ?? []).Select(p => CreateInstanceWithDefaults(p.ParameterType)).ToArray();
        }
        public static Type[] GetDefaultParameterTypes(this MethodInfo method)
        {
            return (method?.GetParameters() ?? []).Select(p => p.ParameterType).ToArray();
        }
        public static object CreateInstanceWithDefaults(this Type type)
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


            object instance;
            try
            {
                instance = Activator.CreateInstance(type); // parametresiz constructor
            }
            catch (MissingMethodException)
            {
                // parametresiz constructor yoksa, en az parametreli constructor'u bul ve default değerlerle oluştur
                var ctor = type.GetConstructors()
                    .OrderBy(c => c.GetParameters().Length)
                    .FirstOrDefault();

                if (ctor == null)
                    return null; // constructor yoksa instance oluşturulamaz

                var args = ctor.GetParameters()
                    .Select(p => CreateInstanceWithDefaults(p.ParameterType))
                    .ToArray();

                instance = ctor.Invoke(args);
            }


            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite && p.GetIndexParameters().Length == 0))
            {
                var propertyType = property.PropertyType;
                var propertyValue = CreateInstanceWithDefaults(propertyType);
                property.SetValue(instance, propertyValue);
            }

            return instance;
        }

        public static string GetJsonSchema(this MethodInfo method)
        {
            try
            {
                if (method == null) return null;

                var parameterTypes = new Dictionary<string, Type>();

                foreach (var param in method.GetParameters())
                {
                    parameterTypes[param.Name] = param.ParameterType;
                }

                if (parameterTypes.Count == 0) return null;

                var schemas = new Dictionary<string, JsonSchema>();

                foreach (var parameter in parameterTypes)
                {
                    var subSchema = JsonSchema.FromType(parameter.Value);
                    subSchema.Title = parameter.Key;
                    schemas.Add(parameter.Key, subSchema);
                }

                var combinedSchema = new JsonSchema()
                {
                    Type = JsonObjectType.Object,
                    Title = method.Name,
                    AllowAdditionalItems = false,
                    AllowAdditionalProperties = false,

                };

                foreach (var jSchema in schemas)
                {
                    combinedSchema.Properties[jSchema.Key] = new JsonSchemaProperty
                    {
                        Type = JsonObjectType.Object,
                        Reference = jSchema.Value,
                        Title = jSchema.Key
                    };

                    combinedSchema.Definitions[jSchema.Key] = jSchema.Value;
                    combinedSchema.Definitions[jSchema.Key].Title = jSchema.Key;
                }


                return combinedSchema.ToJson(Formatting.None);
            }
            catch
            {
                return default;
            }
        }
    }



}
