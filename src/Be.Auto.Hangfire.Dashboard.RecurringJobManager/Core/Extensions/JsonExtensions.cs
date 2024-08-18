using Newtonsoft.Json;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;

public static class JsonExtensions
{
    public static string SerializeObjectToJson(this object @this)
    {
        return  JsonConvert.SerializeObject(@this, new JsonSerializerSettings()
        {
            Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() },
        });
    }
    public static T DeserializeObjectFromJson<T>(this string @this)
    {
        return JsonConvert.DeserializeObject<T>(@this);
    }
}