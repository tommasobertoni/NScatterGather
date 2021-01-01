using System.Collections.Generic;

namespace NScatterGather.Recipients.Invokers
{
    internal interface IRecipientInvoker
    {
        IReadOnlyList<PreparedInvocation<object?>> PrepareInvocations(object request);

        IReadOnlyList<PreparedInvocation<TResult>> PrepareInvocations<TResult>(object request);

        IRecipientInvoker Clone();
    }
}
