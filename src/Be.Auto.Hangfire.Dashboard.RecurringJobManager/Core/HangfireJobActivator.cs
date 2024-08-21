using System;
using System.Linq;
using System.Reflection;
using Hangfire;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;

public class HangfireJobActivator(IServiceProvider serviceProvider) : JobActivator
{
    public override object ActivateJob(Type type)
    {
        var instance = serviceProvider.GetService(type);

        if (instance != null) return instance;

        foreach (var @interface in type.GetInterfaces().OrderByDescending(t => t.GetMethods().Length))
        {
            instance = serviceProvider.GetService(@interface);

            if (instance != null)
            {

                break;
            }
        }

        return instance ?? CreateInstance(type);

    }

    private static object CreateInstance(Type type)
    {
        if (type.IsAbstract && type.IsSealed)
        {
            return type;
        }

        var constructor = type.GetConstructor(Type.EmptyTypes);

        if (constructor != null)
        {
            return Activator.CreateInstance(type);
        }

        var constructors = type.GetConstructors();

        if (constructors.Length == 0)
        {
            return Activator.CreateInstance(type, true);
        }

        var selectedConstructor = constructors.OrderBy(c => c.GetParameters().Length).First();
        var parameters = selectedConstructor.GetParameters();
        var defaultValues = parameters.Select(p =>
        {
            if (p.HasDefaultValue)
            {
                return p.DefaultValue;
            }
            return p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null;
        }).ToArray();

        return selectedConstructor.Invoke(defaultValues);
    }
}