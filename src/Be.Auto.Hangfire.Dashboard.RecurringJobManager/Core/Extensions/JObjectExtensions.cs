using Newtonsoft.Json.Linq;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;

public static class JObjectExtensions
{
    public static void TrimAllStrings(this JToken token)
    {
        if (token.Type == JTokenType.Object)
        {
            foreach (var property in ((JObject)token).Properties())
            {
                TrimAllStrings(property.Value);
            }
        }
        else if (token.Type == JTokenType.Array)
        {
            foreach (var item in (JArray)token)
            {
                TrimAllStrings(item);
            }
        }
        else if (token.Type == JTokenType.String)
        {
            var value = token.ToString().UnescapeMulti();
            ((JValue)token).Value = value;
        }
    }
}