using System.Collections.Generic;
using System.Text.RegularExpressions;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;

internal static class JsonExtensions
{
    public static string SerializeObjectToJson(this object @this)
    {
        return JsonConvert.SerializeObject(@this, new JsonSerializerSettings()
        {
            Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() },
        });
    }
    public static T DeserializeObjectFromJson<T>(this string @this)
    {
        return JsonConvert.DeserializeObject<T>(@this);
    }

    public static bool TryDeserializeObjectFromJson<T>(this string @this, out T result)
    {
        try
        {
            result = JsonConvert.DeserializeObject<T>(@this);
            return true;
        }
        catch
        {


            result = default(T);
            return false;
        }
    }
    public static List<RecurringJobBase> TryDeserializeJobs(this string @this, out bool result)
    {
        try
        {
            var jobs = JsonConvert.DeserializeObject<List<RecurringJobBase>>(@this,new RecurringJobBaseConverter());
            result = true;
            return jobs;
        }
        catch
        {
            result = false;
            return new List<RecurringJobBase>();
        }
    }

    public static bool IsValidJson(this string @this)
    {
        if (string.IsNullOrEmpty(@this)) return false;

        var jsonRegex = new Regex(@"^\s*(\{.*\}|\[.*\])\s*$", RegexOptions.Singleline);

        if (!jsonRegex.IsMatch(@this)) return false;
        
        try
        {
            JToken.Parse(@this);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string UnescapeJson(this string @json)
    {

        while (Regex.IsMatch(@json, @"\\[\\nt\""]"))
        {
            @json = Regex.Unescape(@json);
        }

        while (@json.StartsWith("\"") && @json.EndsWith("\""))
        {
            @json = @json.Substring(1, @json.Length - 2);

        }

        return @json;

    }
}
