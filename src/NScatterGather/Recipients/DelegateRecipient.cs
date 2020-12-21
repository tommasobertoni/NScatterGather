using System;
using NScatterGather.Recipients.Descriptors;
using NScatterGather.Recipients.Invokers;

namespace NScatterGather.Recipients
{
    internal class DelegateRecipient : Recipient
    {
        public Type RequestType { get; }

        public Type ResponseType { get; }

        private readonly DelegateRecipientDescriptor _typedDescriptor;
        private readonly DelegateRecipientInvoker _typedInvoker;

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

            return new DelegateRecipient(descriptor, invoker, name);
        }

        protected DelegateRecipient(
            DelegateRecipientDescriptor descriptor,
            DelegateRecipientInvoker invoker,
            string? name) : base(descriptor, invoker, name, Lifetime.Singleton)
        {
            _typedDescriptor = descriptor;
            _typedInvoker = invoker;

            RequestType = descriptor.RequestType;
            ResponseType = descriptor.ResponseType;
        }

#if NETSTANDARD2_0 || NETSTANDARD2_1
        public override Recipient Clone() => new DelegateRecipient(_typedDescriptor, _typedInvoker, Name);
#else
        public override DelegateRecipient Clone() => new(_typedDescriptor, _typedInvoker, Name);
#endif
    }
}
