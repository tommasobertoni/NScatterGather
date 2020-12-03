using System;
using NScatterGather.Inspection;

namespace NScatterGather.Recipients
{
    internal class InstanceRecipient : Recipient
    {
        public Type Type => _type;

        private readonly Type _type;
        private readonly object _instance;
        private readonly TypeInspector _inspector;

        public InstanceRecipient(
            object instance,
            TypeInspector? inspector = null)
        {
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
            _type = _instance.GetType();
            _inspector = inspector ?? new TypeInspector(_type);
        }

        public InstanceRecipient(
            Type type,
            TypeInspector? inspector = null)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _inspector = inspector ?? new TypeInspector(_type);

            try
            {
                _instance = Activator.CreateInstance(_type)!;
            }
            catch (MissingMethodException mMEx)
            {
                throw new InvalidOperationException($"Could not create a new instance of type '{_type.Name}'.", mMEx);
            }
        }

        protected internal override string GetRecipientName() =>
            _type.Name;

        public override bool CanAccept(Type requestType)
        {
            if (requestType is null)
                throw new ArgumentNullException(nameof(requestType));

            var accepts = _inspector.HasMethodAccepting(requestType);
            return accepts;
        }

        public override bool CanReplyWith(Type requestType, Type responseType)
        {
            if (requestType is null)
                throw new ArgumentNullException(nameof(requestType));

            if (responseType is null)
                throw new ArgumentNullException(nameof(responseType));

            var repliesWith = _inspector.HasMethodReturning(requestType, responseType);
            return repliesWith;
        }

        protected internal override object? Invoke<TRequest>(TRequest request)
        {
            if (!_inspector.TryGetMethodAccepting<TRequest>(out var method))
                throw new InvalidOperationException(
                    $"Type '{GetRecipientName()}' doesn't support accepting requests " +
                    $"of type '{typeof(TRequest).Name}'.");

            var response = method.Invoke(_instance, new object?[] { request });
            return response;
        }

        protected internal override object? Invoke<TRequest, TResponse>(TRequest request)
        {
            if (!_inspector.TryGetMethodReturning<TRequest, TResponse>(out var method))
                throw new InvalidOperationException(
                    $"Type '{GetRecipientName()}' doesn't support accepting " +
                    $"requests of type '{typeof(TRequest).Name}' and " +
                    $"returning '{typeof(TResponse).Name}'.");

            var response = method.Invoke(_instance, new object?[] { request });
            return response;
        }
    }
}
