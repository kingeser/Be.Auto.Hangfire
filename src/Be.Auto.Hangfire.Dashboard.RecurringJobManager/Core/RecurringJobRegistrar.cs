using Hangfire.States;
using System;
using System.Linq;
using System.Reflection;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Attributes;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models.Enums;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core
{
    internal static class RecurringJobRegistrar
    {
        internal static void Register()
        {
            foreach (var assembly in AssemblyInfoStorage.Assemblies)
            {
                foreach (var type in assembly.TypeMethods)
                {
                    foreach (var method in type.Value.Where(t => t.IsDefined(typeof(RecurringJobAttribute))))
                    {
                        var attribute = method.GetCustomAttribute<RecurringJobAttribute>(false);

                        if (attribute == null) continue;

                        if (!RecurringJobAgent.IsValidJobId(attribute.RecurringJobId) && !RecurringJobAgent.IsValidJobId(attribute.RecurringJobId, RecurringJobAgent.TagStopJob))
                        {
                            new RecurringJobMethodCall()
                            {
                                Id = attribute.RecurringJobId,
                                TimeZoneId = attribute.TimeZone.Id,
                                Class = type.Key.FullName,
                                Method = method.Name,
                                Cron = attribute.Cron,
                                CreatedAt = DateTime.Now,
                                JobState = EnqueuedState.StateName,
                                MisfireHandlingMode = attribute.MisfireHandlingMode,
                                Removed = false,
                                MethodParameters = method.GetParameterNamesAndDefaults().SerializeObjectToJson(),
                                Error = string.Empty,
                                LastJobState = string.Empty,
                                LastExecution = string.Empty,
                                LastJobId = string.Empty,
                                NextExecution = string.Empty
                            }.Register();
                        }

                    }
                }
            }
        }
    }
}
