using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NScatterGather.Inspection;
using NScatterGather.Recipients.Factories;

namespace NScatterGather.Recipients.Invokers
{
    internal class InstanceRecipientInvoker : IRecipientInvoker
    {
        private static readonly MethodAnalyzer _methodAnalyzer = new MethodAnalyzer();

        private readonly TypeInspector _inspector;
        private readonly IRecipientFactory _factory;
        private readonly CollisionStrategy _collisionStrategy;

        public InstanceRecipientInvoker(
            TypeInspector inspector,
            IRecipientFactory factory,
            CollisionStrategy collisionStrategy)
        {
            _inspector = inspector;
            _factory = factory;
            _collisionStrategy = collisionStrategy;
        }

        public IReadOnlyList<PreparedInvocation<object?>> PrepareInvocations(
            object request,
            CancellationToken cancellationToken = default)
        {
            if (!_inspector.TryGetMethodsAccepting(request.GetType(), _collisionStrategy, out var methods))
                throw new InvalidOperationException(
                    $"Type '{_inspector.Type.Name}' doesn't support accepting requests " +
                    $"of type '{request.GetType().Name}'.");

            var preparedInvocations = methods.Select(method =>
            {
                var recipientInstance = _factory.Get();

                Func<object?> invocation = _methodAnalyzer.AcceptsCancellationToken(method)
                    ? () => method.Invoke(recipientInstance, new object?[] { request, cancellationToken })
                    : () => method.Invoke(recipientInstance, new object?[] { request });

                return new PreparedInvocation<object?>(invocation);
            });

            return preparedInvocations.ToArray();
        }

        public IReadOnlyList<PreparedInvocation<TResult>> PrepareInvocations<TResult>(
            object request,
            CancellationToken cancellationToken = default)
        {
            if (!_inspector.TryGetMethodsReturning(request.GetType(), typeof(TResult), _collisionStrategy, out var methods))
                throw new InvalidOperationException(
                    $"Type '{_inspector.Type.Name}' doesn't support accepting " +
                    $"requests of type '{request.GetType().Name}' and " +
                    $"returning '{typeof(TResult).Name}'.");

            var preparedInvocations = methods.Select(method =>
            {
                var recipientInstance = _factory.Get();

                Func<object?> invocation = _methodAnalyzer.AcceptsCancellationToken(method)
                    ? () => method.Invoke(recipientInstance, new object?[] { request, cancellationToken })
                    : () => method.Invoke(recipientInstance, new object?[] { request });

                return new PreparedInvocation<TResult>(invocation);
            });

            return preparedInvocations.ToArray();
        }

        public IRecipientInvoker Clone()
        {
            var factory = _factory.Clone();
            return new InstanceRecipientInvoker(_inspector, factory, _collisionStrategy);
        }
    }
}
