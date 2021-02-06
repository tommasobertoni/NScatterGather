using System.Collections.Generic;
using System.Threading;

namespace NScatterGather.Recipients.Invokers
{
    internal interface IRecipientInvoker
    {
        IReadOnlyList<PreparedInvocation<object?>> PrepareInvocations(
            object request,
            CancellationToken cancellationToken = default);

        IReadOnlyList<PreparedInvocation<TResult>> PrepareInvocations<TResult>(
            object request,
            CancellationToken cancellationToken = default);

        IRecipientInvoker Clone();
    }
}
