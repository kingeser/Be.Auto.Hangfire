using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;

internal static class DictionaryExtensions
{
    public static T BindFromDictionary<T>(this Dictionary<string, string> dictionary) where T : new()
    {
        dictionary ??= new Dictionary<string, string>();

        var obj = new T();

        var type = typeof(T);

        if (dictionary.Count == 0) return obj;

        foreach (var property in type.GetProperties().Where(t => t.CanWrite))
        {
            if (!dictionary.TryGetValue(property.Name, out var value)) continue;

            value = value.UnescapeMulti();
          
            var converter = TypeDescriptor.GetConverter(property.PropertyType);

            try
            {
                if (converter.IsValid(value))
                {
                    var convertedValue = converter.ConvertFromString(value);
                    property.SetValue(obj, convertedValue);
                }
                else if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var listType = property.PropertyType.GetGenericArguments()[0];
                    var list = Newtonsoft.Json.JsonConvert.DeserializeObject(value, typeof(List<>).MakeGenericType(listType));
                    property.SetValue(obj, list);
                }
            }
            catch
            {
                property.SetValue(obj, null);
            }
        }

        return obj;
    }
}