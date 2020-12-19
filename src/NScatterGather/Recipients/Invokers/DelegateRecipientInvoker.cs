using System;
using NScatterGather.Recipients.Descriptors;

namespace NScatterGather.Recipients.Invokers
{
    internal class DelegateRecipientInvoker : IRecipientInvoker
    {
        private readonly DelegateRecipientDescriptor _descriptor;
        private readonly Func<object, object?> _delegate;

        public DelegateRecipientInvoker(
            DelegateRecipientDescriptor descriptor,
            Func<object, object?> @delegate)
        {
            _descriptor = descriptor;
            _delegate = @delegate;
        }

        public PreparedInvocation<object?> PrepareInvocation(object request)
        {
            if (!_descriptor.CanAccept(request.GetType()))
                throw new InvalidOperationException(
                    $"Delegate '{_delegate}' doesn't support accepting requests " +
                    $"of type '{request.GetType().Name}'.");

            return new PreparedInvocation<object?>(() => _delegate(request!));
        }

        public PreparedInvocation<TResult> PrepareInvocation<TResult>(object request)
        {
            if (!_descriptor.CanReplyWith(request.GetType(), typeof(TResult)))
                throw new InvalidOperationException(
                    $"Type '{_delegate}' doesn't support accepting " +
                    $"requests of type '{request.GetType().Name}' and " +
                    $"returning '{typeof(TResult).Name}'.");

            return new PreparedInvocation<TResult>(() => (TResult)_delegate(request)!);
        }
    }
}
