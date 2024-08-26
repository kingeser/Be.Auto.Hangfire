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

    public static bool IsValidJson(this string @this)
    {
        if (string.IsNullOrEmpty(@this)) return false;
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
}