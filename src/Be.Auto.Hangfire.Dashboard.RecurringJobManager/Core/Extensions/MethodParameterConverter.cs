using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Hangfire.Dashboard;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;

internal class MethodParameterConverter(MethodInfo methodInfo) : JsonConverter
{
    private readonly ParameterInfo[] _parameters = methodInfo.GetParameters();
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(object[]);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {

        var jsonObject = GetJsonObject(reader);

        var parameterValues = new object[_parameters.Length];

        for (var i = 0; i < _parameters.Length; i++)
        {
            var parameter = _parameters[i];

            var jsonProperty = jsonObject[parameter.Name];

            if (jsonProperty != null)
            {
                parameterValues[i] = jsonProperty.ToObject(parameter.ParameterType, serializer);
            }
            else
            {
                parameterValues[i] = parameter.ParameterType.CreateInstanceWithDefaults();
            }
        }

        return parameterValues;
    }

    private static JObject GetJsonObject(JsonReader reader)
    {
        if (reader.Value?.GetType() != typeof(string)) return JObject.Load(reader);

        var readerValue = reader.Value?.ToString() ?? "{}";

        readerValue = readerValue.UnescapeMulti();

        return JObject.Parse(readerValue);

    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteStartObject();

        var valueArray = (object[])value;

        if (valueArray != null)
        {
            for (var i = 0; i < valueArray.Length; i++)
            {
                writer.WritePropertyName(_parameters[i].Name);

                serializer.Serialize(writer, valueArray[i], _parameters[i].ParameterType);
            }
        }

        writer.WriteEndObject();
    }
}