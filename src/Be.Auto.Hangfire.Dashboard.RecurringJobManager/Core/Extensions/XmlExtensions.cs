using System.Xml;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;

public static class XmlExtensions
{

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