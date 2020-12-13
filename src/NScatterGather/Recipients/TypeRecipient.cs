using System;
using System.Reflection;
using NScatterGather.Inspection;

namespace NScatterGather.Recipients
{
    internal class TypeRecipient : Recipient
    {
        public Type Type => _type;

        private readonly Type _type;
        private readonly Func<object> _factory;
        private readonly TypeInspector _inspector;

        public static TypeRecipient Create<TRecipient>(
            Func<TRecipient>? customFactory = null,
            string? name = null)
        {
            Func<object> factory = customFactory is not null
                ? () => customFactory()
                : HasADefaultConstructor<TRecipient>()
                    ? () => Activator.CreateInstance(typeof(TRecipient))!
                    : throw new ArgumentException($"Type '{typeof(TRecipient).Name}' is missing a public, parameterless constructor.");

            return new TypeRecipient(typeof(TRecipient), factory, name);

            // Local functions.

            static bool HasADefaultConstructor<T>()
            {
                var defaultContructor = typeof(T).GetConstructor(Type.EmptyTypes);
                return defaultContructor is not null;
            }
        }

        protected TypeRecipient(
            Type type,
            Func<object> factory,
            string? name = null) : base(name)
        {
            _type = type;
            _factory = factory;
            _inspector = new TypeInspector(type);
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

        protected internal override object? Invoke(object request)
        {
            if (!_inspector.TryGetMethodAccepting(request.GetType(), out var method))
                throw new InvalidOperationException(
                    $"Type '{GetRecipientName()}' doesn't support accepting requests " +
                    $"of type '{request.GetType().Name}'.");

            var recipientInstance = GetRecipientInstance();
            var response = method.Invoke(recipientInstance, new object?[] { request });
            return response;
        }

        protected internal override object? Invoke<TResponse>(object request)
        {
            if (!_inspector.TryGetMethodReturning(request.GetType(), typeof(TResponse), out var method))
                throw new InvalidOperationException(
                    $"Type '{GetRecipientName()}' doesn't support accepting " +
                    $"requests of type '{request.GetType().Name}' and " +
                    $"returning '{typeof(TResponse).Name}'.");

            var recipientInstance = GetRecipientInstance();
            var response = method.Invoke(recipientInstance, new object?[] { request });
            return response;
        }

        protected virtual object GetRecipientInstance()
        {
            var instance = _factory();
            return instance;
        }
    }
}
