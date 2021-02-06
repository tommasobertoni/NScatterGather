using System;
using System.Collections.Generic;
using System.Threading;
using NScatterGather.Recipients.Descriptors;

namespace NScatterGather.Recipients.Invokers
{
    internal class DelegateRecipientInvoker : IRecipientInvoker
    {
        private readonly DelegateRecipientDescriptor _descriptor;
        private readonly Func<object, CancellationToken, object?> _delegate;

        public DelegateRecipientInvoker(
            DelegateRecipientDescriptor descriptor,
            Func<object, object?> @delegate)
        {
            _descriptor = descriptor;
            _delegate = (request, cancellationToken) => @delegate(request);
        }

        public DelegateRecipientInvoker(
            DelegateRecipientDescriptor descriptor,
            Func<object, CancellationToken, object?> @delegate)
        {
            _descriptor = descriptor;
            _delegate = @delegate;
        }

        public IReadOnlyList<PreparedInvocation<object?>> PrepareInvocations(
            object request,
            CancellationToken cancellationToken = default)
        {
            if (!_descriptor.CanAccept(request.GetType(), CollisionStrategy.IgnoreRecipient))
                throw new InvalidOperationException(
                    $"Delegate '{_delegate}' doesn't support accepting requests " +
                    $"of type '{request.GetType().Name}'.");

            var preparedInvocation = new PreparedInvocation<object?>(
                () => _delegate(request!, cancellationToken),
                _descriptor.AcceptsCancellationToken);

            return new[] { preparedInvocation };
        }

        public IReadOnlyList<PreparedInvocation<TResult>> PrepareInvocations<TResult>(
            object request,
            CancellationToken cancellationToken = default)
        {
            if (!_descriptor.CanReplyWith(request.GetType(), typeof(TResult), CollisionStrategy.IgnoreRecipient))
                throw new InvalidOperationException(
                    $"Type '{_delegate}' doesn't support accepting " +
                    $"requests of type '{request.GetType().Name}' and " +
                    $"returning '{typeof(TResult).Name}'.");

            var preparedInvocation = new PreparedInvocation<TResult>(
                () => _delegate(request, cancellationToken)!,
                _descriptor.AcceptsCancellationToken);

            return new[] { preparedInvocation };
        }

        public IRecipientInvoker Clone() =>
            new DelegateRecipientInvoker(_descriptor, _delegate);
    }
}
