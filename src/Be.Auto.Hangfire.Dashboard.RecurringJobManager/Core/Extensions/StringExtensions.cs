using System.Text.RegularExpressions;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;

internal static class StringExtensions
{
    public static string CleanVersionDetails(this string content)
    {
        return Regex.Replace(content, @"(Version|Culture|PublicKeyToken)=.*?(\s*,|\s*}|\s*$)", string.Empty);
    }
}