using System;
using Hangfire;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;

public class HangfireJobActivator(IServiceProvider serviceProvider) : JobActivator
{
    public override object ActivateJob(Type type)
    {
        var instance = serviceProvider.GetService(type);

        if (instance != null) return instance;

        foreach (var @interface in type.GetInterfaces())
        {
            instance = serviceProvider.GetService(@interface);

            if (instance != null) break;
        }

        return instance;

    }
}