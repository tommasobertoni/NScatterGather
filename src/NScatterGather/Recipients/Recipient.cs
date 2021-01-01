using System;
using System.Collections.Generic;
using System.Linq;
using NScatterGather.Recipients.Descriptors;
using NScatterGather.Recipients.Invokers;
using NScatterGather.Recipients.Run;

namespace NScatterGather.Recipients
{
    internal abstract class Recipient
    {
        public string? Name { get; }

        public Lifetime Lifetime { get; }

        public CollisionStrategy CollisionStrategy { get; }

        protected readonly IRecipientDescriptor _descriptor;
        protected readonly IRecipientInvoker _invoker;

        public Recipient(
            IRecipientDescriptor descriptor,
            IRecipientInvoker invoker,
            string? name,
            Lifetime lifetime,
            CollisionStrategy collisionStrategy)
        {
            _descriptor = descriptor;
            _invoker = invoker;

            Name = name;
            Lifetime = lifetime;
            CollisionStrategy = collisionStrategy;
        }

        public bool CanAccept(Type requestType) =>
            _descriptor.CanAccept(requestType, CollisionStrategy);

        public bool CanReplyWith(Type requestType, Type responseType) =>
            _descriptor.CanReplyWith(requestType, responseType, CollisionStrategy);

        public IReadOnlyList<RecipientRun<object?>> Accept(object request)
        {
            var preparedInvocations = _invoker.PrepareInvocations(request);

            var runners = preparedInvocations.Select(preparedInvocation =>
                new RecipientRun<object?>(this, preparedInvocation));

            return runners.ToArray();
        }

        public IReadOnlyList<RecipientRun<TResponse>> ReplyWith<TResponse>(object request)
        {
            var preparedInvocations = _invoker.PrepareInvocations<TResponse>(request);

            var runners = preparedInvocations.Select(preparedInvocation =>
                new RecipientRun<TResponse>(this, preparedInvocation));

            return runners.ToArray();
        }

        public abstract Recipient Clone();
    }
}
