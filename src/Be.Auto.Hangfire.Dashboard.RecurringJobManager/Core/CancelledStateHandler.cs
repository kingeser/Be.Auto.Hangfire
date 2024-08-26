using Hangfire.States;
using Hangfire.Storage;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core;

internal class CancelledStateHandler : IStateHandler
{
    public void Apply(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
       
        transaction.IncrementCounter("stats:cancelled");
    }

    public void Unapply(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        transaction.DecrementCounter("stats:cancelled");
    }

    public string StateName => "Cancelled";
}

