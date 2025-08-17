using System.IO;
using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;

internal static class XmlExtensions
{

    public static string SerializeObjectToXml(this object obj)
    {
        switch (obj)
        {
            case null:
                return string.Empty;
            case string:
                return obj.ToString();

            default:

                {
                    try
                    {
                        var serializer = new XmlSerializer(obj.GetType());
                        using var sw = new StringWriter();
                        serializer.Serialize(sw, obj);
                        var xml = sw.ToString();
                        return xml;
                    }
                    catch (Exception e)
                    {
                        var json = JsonConvert.SerializeObject(obj);

                        try
                        {
                            return JsonConvert.DeserializeXmlNode(json)?.OuterXml ?? string.Empty;
                        }
                        catch
                        {
                            return JsonConvert.DeserializeXmlNode("{\"Root\":" + json + "}")?.OuterXml ?? string.Empty;
                        }

                    }
                }

        }

    }
    public static bool IsValidXml(this string @this)
    {

        try
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(@this);
            return true;
        }
        catch
        {
            return false;
        }
    }
}