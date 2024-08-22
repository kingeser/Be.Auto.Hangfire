using System.Text;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;

using System;

public static class ExceptionExtensions
{
    /// <summary>
    /// Gets all exception messages recursively, including inner exceptions.
    /// </summary>
    /// <param name="exception">The exception to extract messages from.</param>
    /// <returns>A string containing all exception messages.</returns>
    public static string GetAllMessages(this Exception exception)
    {
        if (exception == null) throw new ArgumentNullException(nameof(exception));

        var sb = new StringBuilder();
        GetAllMessagesRecursive(exception, sb);

        return sb.ToString();
    }

    private static void GetAllMessagesRecursive(Exception exception, StringBuilder sb)
    {
        while (true)
        {
            if (exception == null) return;

            // Append the current exception message
            sb.AppendLine(exception.Message);

            // Recurse through the inner exceptions, if any
            if (exception.InnerException == null) return;
            sb.AppendLine("Inner Exception:");
            exception = exception.InnerException;
        }
    }
}