using System;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;

public class WebRequestJobOption
{
    public TimeSpan TimeOut { get; set; } = TimeSpan.FromSeconds(30);

}