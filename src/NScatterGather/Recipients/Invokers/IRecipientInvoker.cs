namespace NScatterGather.Recipients.Invokers
{
    internal interface IRecipientInvoker
    {
        PreparedInvocation<object?> PrepareInvocation(object request);

        PreparedInvocation<TResult> PrepareInvocation<TResult>(object request);
    }
}
