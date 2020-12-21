using System;
using NScatterGather.Recipients.Descriptors;
using NScatterGather.Recipients.Invokers;
using NScatterGather.Recipients.Run;

namespace NScatterGather.Recipients
{
    internal abstract class Recipient
    {
        public string? Name { get; }

        public Lifetime Lifetime { get; }

        protected readonly IRecipientDescriptor _descriptor;
        protected readonly IRecipientInvoker _invoker;

        public Recipient(
            IRecipientDescriptor descriptor,
            IRecipientInvoker invoker,
            string? name,
            Lifetime lifetime)
        {
            _descriptor = descriptor;
            _invoker = invoker;

            Name = name;
            Lifetime = lifetime;
        }

        public bool CanAccept(Type requestType) =>
            _descriptor.CanAccept(requestType);

        public bool CanReplyWith(Type requestType, Type responseType) =>
            _descriptor.CanReplyWith(requestType, responseType);

        public RecipientRun<object?> Accept(object request)
        {
            var preparedInvocation = _invoker.PrepareInvocation(request);
            var run = new RecipientRun<object?>(this, preparedInvocation);
            return run;
        }

        public RecipientRun<TResponse> ReplyWith<TResponse>(object request)
        {
            var preparedInvocation = _invoker.PrepareInvocation<TResponse>(request);
            var run = new RecipientRun<TResponse>(this, preparedInvocation);
            return run;
        }

        public abstract Recipient Clone();
    }
}
