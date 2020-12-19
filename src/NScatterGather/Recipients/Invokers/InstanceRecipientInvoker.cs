using System;
using NScatterGather.Inspection;
using NScatterGather.Recipients.Factories;

namespace NScatterGather.Recipients.Invokers
{
    internal class InstanceRecipientInvoker : IRecipientInvoker
    {
        private readonly TypeInspector _inspector;
        private readonly IRecipientFactory _factory;

        public InstanceRecipientInvoker(
            TypeInspector inspector,
            IRecipientFactory factory)
        {
            _inspector = inspector;
            _factory = factory;
        }

        public PreparedInvocation<object?> PrepareInvocation(object request)
        {
            if (!_inspector.TryGetMethodAccepting(request.GetType(), out var method))
                throw new InvalidOperationException(
                    $"Type '{_inspector.Type.Name}' doesn't support accepting requests " +
                    $"of type '{request.GetType().Name}'.");

            var recipientInstance = _factory.Get();

            return new PreparedInvocation<object?>(invocation: () =>
                method.Invoke(recipientInstance, new object?[] { request }));
        }

        public PreparedInvocation<TResult> PrepareInvocation<TResult>(object request)
        {
            if (!_inspector.TryGetMethodReturning(request.GetType(), typeof(TResult), out var method))
                throw new InvalidOperationException(
                    $"Type '{_inspector.Type.Name}' doesn't support accepting " +
                    $"requests of type '{request.GetType().Name}' and " +
                    $"returning '{typeof(TResult).Name}'.");

            var recipientInstance = _factory.Get();

            return new PreparedInvocation<TResult>(invocation: () =>
                (TResult)method.Invoke(recipientInstance, new object?[] { request })!);
        }
    }
}
