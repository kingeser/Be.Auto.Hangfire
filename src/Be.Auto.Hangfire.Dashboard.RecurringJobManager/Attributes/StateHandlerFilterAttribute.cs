using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace Be.Auto.Hangfire.Dashboard.RecurringJobManager.Attributes;

public class StateHandlerFilterAttribute(IStateHandler stateHandler) : JobFilterAttribute, IApplyStateFilter, IElectStateFilter
{
    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.NewState.Name == stateHandler.StateName)
        {
            stateHandler.Apply(context, transaction);
        }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.OldStateName == stateHandler.StateName)
        {
            stateHandler.Unapply(context, transaction);
        }
    }

    public void OnStateElection(ElectStateContext context)
    {

    }
}