using System;
using NScatterGather.Recipients.Descriptors;
using NScatterGather.Recipients.Invokers;

namespace NScatterGather.Recipients
{
    internal class DelegateRecipient : Recipient
    {
        public Type RequestType { get; }

        public Type ResponseType { get; }

        public static DelegateRecipient Create<TRequest, TResponse>(
            Func<TRequest, TResponse> @delegate,
            string? name)
        {
            if (@delegate is null)
                throw new ArgumentNullException(nameof(@delegate));

            object? delegateInvoker(object request)
            {
                var typedRequest = (TRequest)request;
                TResponse response = @delegate(typedRequest);
                return response;
            }

            var descriptor = new DelegateRecipientDescriptor(typeof(TRequest), typeof(TResponse));
            var invoker = new DelegateRecipientInvoker(descriptor, delegateInvoker);

            return new DelegateRecipient(descriptor.RequestType, descriptor.ResponseType, descriptor, invoker, name);
        }

        protected DelegateRecipient(
            Type requestType,
            Type responseType,
            IRecipientDescriptor descriptor,
            IRecipientInvoker invoker,
            string? name)
            : base(descriptor, invoker, name, Lifetime.Singleton, CollisionStrategy.IgnoreRecipient)
        {
            RequestType = requestType;
            ResponseType = responseType;
        }

#if NETSTANDARD2_0 || NETSTANDARD2_1
        public override Recipient Clone() =>
#else
        public override DelegateRecipient Clone() =>
#endif
            new DelegateRecipient(RequestType, ResponseType, _descriptor, _invoker, Name);
    }
}
