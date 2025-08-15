using System;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;

internal class RecurringJobBaseConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(RecurringJobBase);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var jobObject = JObject.Load(reader,new JsonLoadSettings()
        {
            DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace,
            CommentHandling = CommentHandling.Ignore,
            LineInfoHandling = LineInfoHandling.Load,
            
        });

        jobObject.TrimAllStrings();

        var jobType = jobObject["JobType"]?.ToString();

        RecurringJobBase job;
        switch (jobType)
        {
            case "MethodCall":
                job = new RecurringJobMethodCall();
                break;
            case "WebRequest":
                job = new RecurringJobWebRequest();
                break;
            default:
                throw new NotSupportedException($"JobType '{jobType}' is not supported.");
        }

        serializer.Populate(jobObject.CreateReader(), job);

        return job;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}