using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;

public static class JsonSchemaExtensions
{
    public static JSchema GenerateJSchema(this string jsonString)
    {
        var jsonObject = JObject.Parse(jsonString);
        var schema = new JSchema
        {
            Type = JSchemaType.Object
        };

        foreach (var property in jsonObject.Properties())
        {
            var propertySchema = new JSchema();

            switch (property.Value.Type)
            {
                case JTokenType.String:
                    propertySchema.Type = JSchemaType.String;

                    if (DateTime.TryParse(property.Value.ToString(), out DateTime _))
                    {
                        propertySchema.Format = "date-time";
                    }
                    break;

                case JTokenType.Integer:
                    propertySchema.Type = JSchemaType.Integer;
                    break;

                case JTokenType.Float:
                    propertySchema.Type = JSchemaType.Number;
                    break;

                case JTokenType.Boolean:
                    propertySchema.Type = JSchemaType.Boolean;
                    break;

                case JTokenType.Object:
                    propertySchema.Type = JSchemaType.Object;
                    break;

                case JTokenType.Array:
                    propertySchema.Type = JSchemaType.Array;


                    if (property.Value.HasValues)
                    {
                        var firstItem = property.Value.First;
                        if (firstItem != null)
                        {
                            propertySchema.Items.Add(firstItem.ToString().GenerateJSchema());
                        }
                    }
                    break;

                case JTokenType.Null:
                    propertySchema.Type = JSchemaType.Null;
                    break;

                case JTokenType.Date:
                    propertySchema.Type = JSchemaType.String;
                    propertySchema.Format = "date-time";
                    break;

                case JTokenType.Bytes:
                    propertySchema.Type = JSchemaType.String;
                    propertySchema.Format = "byte";
                    break;

                case JTokenType.Guid:
                    propertySchema.Type = JSchemaType.String;
                    propertySchema.Format = "uuid";
                    break;

                case JTokenType.Uri:
                    propertySchema.Type = JSchemaType.String;
                    propertySchema.Format = "uri";
                    break;

                case JTokenType.TimeSpan:
                    propertySchema.Type = JSchemaType.String;
                    propertySchema.Format = "time-span";
                    break;


                case JTokenType.None:
                case JTokenType.Constructor:
                case JTokenType.Property:
                case JTokenType.Comment:
                case JTokenType.Undefined:
                case JTokenType.Raw:
                default:
                    propertySchema.Type = JSchemaType.String;
                    break;
            }

            schema.Properties.Add(property.Name, propertySchema);
        }

        return schema;
    }
}