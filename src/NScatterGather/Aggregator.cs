﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NScatterGather.Invocations;
using NScatterGather.Recipients;

namespace NScatterGather
{
    public class Aggregator
    {
        private readonly RecipientsCollection _recipients;

        public Aggregator(RecipientsCollection recipients) =>
            _recipients = recipients;

        public async Task<AggregatedResponse<object?>> Send<TRequest>(
            TRequest request,
            CancellationToken cancellationToken = default)
        {
            var recipients = _recipients.ListRecipientsAccepting<TRequest>();

            var invocations = await Invoke(recipients, request, cancellationToken)
                .ConfigureAwait(false);

            return new AggregatedResponse<object?>(invocations);
        }

        private async Task<IReadOnlyList<LiveInvocationHolder<object?>>> Invoke<TRequest>(
            IReadOnlyList<Recipient> recipients,
            TRequest request,
            CancellationToken cancellationToken)
        {
            var invocations = recipients
                .Select(p => new LiveInvocationHolder<object?>(p, () => p.Accept(request)))
                .ToArray();

            var tasks = invocations.Select(x => x.Task);
            var allTasksCompleted = Task.WhenAll(tasks);

            if (allTasksCompleted.IsCompletedSuccessfully())
                return invocations;

            using (var cancellation = new CancellationTokenTaskSource<object?[]>(cancellationToken))
                await Task.WhenAny(allTasksCompleted, cancellation.Task).ConfigureAwait(false);

            return invocations;
        }

        public async Task<AggregatedResponse<TResponse>> Send<TRequest, TResponse>(
            TRequest request,
            CancellationToken cancellationToken = default)
        {
            var recipients = _recipients.ListRecipientsReplyingWith<TRequest, TResponse>();

            var invocations = await Invoke<TRequest, TResponse>(recipients, request, cancellationToken)
                .ConfigureAwait(false);

            return new AggregatedResponse<TResponse>(invocations);
        }

        private async Task<IReadOnlyList<LiveInvocationHolder<TResponse>>> Invoke<TRequest, TResponse>(
            IReadOnlyList<Recipient> recipients,
            TRequest request,
            CancellationToken cancellationToken)
        {
            var invocations = recipients
                .Select(p => new LiveInvocationHolder<TResponse>(p, () => p.ReplyWith<TRequest, TResponse>(request)))
                .ToArray();

            var tasks = invocations.Select(x => x.Task);
            var allTasksCompleted = Task.WhenAll(tasks);

            if (allTasksCompleted.IsCompletedSuccessfully())
                return invocations;

            using (var cancellation = new CancellationTokenTaskSource<TResponse[]>(cancellationToken))
                await Task.WhenAny(allTasksCompleted, cancellation.Task).ConfigureAwait(false);

            return invocations;
        }
    }
}
