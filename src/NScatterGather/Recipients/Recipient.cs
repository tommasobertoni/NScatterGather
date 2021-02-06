using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NScatterGather.Recipients.Descriptors;
using NScatterGather.Recipients.Invokers;
using NScatterGather.Recipients.Run;

namespace NScatterGather.Recipients
{
    internal abstract class Recipient
    {
        public Guid Id { get; } = Guid.NewGuid();

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

        public IReadOnlyList<RecipientRunner<object?>> Accept(
            object request,
            CancellationToken cancellationToken = default)
        {
            var preparedInvocations = _invoker.PrepareInvocations(request, cancellationToken);

            var runners = preparedInvocations.Select(preparedInvocation =>
                new RecipientRunner<object?>(this, preparedInvocation));

            return runners.ToArray();
        }

        public IReadOnlyList<RecipientRunner<TResponse>> ReplyWith<TResponse>(
            object request,
            CancellationToken cancellationToken = default)
        {
            var preparedInvocations = _invoker.PrepareInvocations<TResponse>(request, cancellationToken);

            var runners = preparedInvocations.Select(preparedInvocation =>
                new RecipientRunner<TResponse>(this, preparedInvocation));

            return runners.ToArray();
        }

        public abstract Recipient Clone();
    }
}
