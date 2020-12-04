using System;

namespace NScatterGather.Recipients
{
    internal class DelegateRecipient : Recipient
    {
        private readonly Func<object, object?> _delegate;

        internal Type In => _inType;

        internal Type Out => _outType;

        private readonly Type _inType;
        private readonly Type _outType;

        public static DelegateRecipient Create<TRequest, TResponse>(Func<TRequest, TResponse> @delegate)
        {
            if (@delegate is null)
                 throw new ArgumentNullException(nameof(@delegate));

            object? delegateInvoker(object @in)
            {
                var request = (TRequest)@in;
                TResponse response = @delegate(request);
                return response;
            }

            return new DelegateRecipient(delegateInvoker, typeof(TRequest), typeof(TResponse));
        }

        internal DelegateRecipient(
            Func<object, object?> @delegate,
            Type inType,
            Type outType)
        {
            _delegate = @delegate;
            _inType = inType;
            _outType = outType;
        }

        protected internal override string GetRecipientName() =>
            _delegate.ToString() ?? "delegate";

        public override bool CanAccept(Type requestType) =>
            Match(_inType, requestType);

        public override bool CanReplyWith(Type requestType, Type responseType) =>
            Match(_inType, requestType) && Match(_outType, responseType);

        private bool Match(Type target, Type actual)
        {
            if (target == actual)
                return true;

            var nonNullableType = Nullable.GetUnderlyingType(target);
            if (nonNullableType is not null && nonNullableType == actual)
                return true;

            return false;
        }

        protected internal override object? Invoke(object request)
        {
            if (!CanAccept(request.GetType()))
                throw new InvalidOperationException(
                    $"Type '{GetRecipientName()}' doesn't support accepting requests " +
                    $"of type '{request.GetType().Name}'.");

            return _delegate(request!);
        }

        protected internal override object? Invoke<TResponse>(object request)
        {
            if (!CanReplyWith(request.GetType(), typeof(TResponse)))
                throw new InvalidOperationException(
                    $"Type '{GetRecipientName()}' doesn't support accepting " +
                    $"requests of type '{request.GetType().Name}' and " +
                    $"returning '{typeof(TResponse).Name}'.");

            return _delegate(request!);
        }
    }
}
