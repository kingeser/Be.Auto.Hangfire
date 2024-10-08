﻿using System;
using System.Collections.Generic;
using Hangfire.Common;
using Hangfire.States;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;

internal class CancelledState(DateTime createdAt) : IState
{
    public Dictionary<string, string> SerializeData()
    {
        var result = new Dictionary<string, string>
        {
            { "CreatedAt", JobHelper.SerializeDateTime(createdAt) },
            { "CancelledAt", JobHelper.SerializeDateTime(DateTime.UtcNow) }
        };

        return result;
    }
    public string Name => "Cancelled";
    public string Reason => "It is not allowed to perform multiple same tasks.";
    public bool IsFinal => true;
    public bool IgnoreJobLoadException => true;
}