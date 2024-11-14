using System;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;

public class RecurringJobException : Exception
{

    public RecurringJobException(string message) : base(message)
    {

    }
    public RecurringJobException(string message, Exception innerException) : base(message, innerException)
    {

    }

  
}