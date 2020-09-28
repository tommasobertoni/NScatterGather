using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
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

            var invocations = await Invoke(recipients, request, cancellationToken);

            return new AggregatedResponse<object?>(invocations);
        }

        private async Task<IReadOnlyList<Invocation<object?>>> Invoke<TRequest>(
            IReadOnlyList<Recipient> recipients,
            TRequest request,
            CancellationToken cancellationToken)
        {
            var invocations = recipients
                .Select(p => new Invocation<object?>(p, p.Accept(request)))
                .ToArray();

            var tasks = invocations.Select(x => x.Task);
            var allTasksCompleted = Task.WhenAll(tasks);

            if (allTasksCompleted.IsCompletedSuccessfully)
                return invocations;

            using (var cts = new CancellationTokenTaskSource<object?[]>(cancellationToken))
                await Task.WhenAny(allTasksCompleted, cts.Task);

            return invocations;
        }

        public async Task<AggregatedResponse<TResponse>> Send<TRequest, TResponse>(
            TRequest request,
            CancellationToken cancellationToken = default)
        {
            var recipients = _recipients.ListRecipientsReplyingWith<TRequest, TResponse>();

            var invocations = await Invoke<TRequest, TResponse>(recipients, request, cancellationToken);

            return new AggregatedResponse<TResponse>(invocations);
        }

        private async Task<IReadOnlyList<Invocation<TResponse>>> Invoke<TRequest, TResponse>(
            IReadOnlyList<Recipient> recipients,
            TRequest request,
            CancellationToken cancellationToken)
        {
            var invocations = recipients
                .Select(p => new Invocation<TResponse>(p, p.ReplyWith<TRequest, TResponse>(request)))
                .ToArray();

            var tasks = invocations.Select(x => x.Task);
            var allTasksCompleted = Task.WhenAll(tasks);

            if (allTasksCompleted.IsCompletedSuccessfully)
                return invocations;

            using (var cts = new CancellationTokenTaskSource<TResponse[]>(cancellationToken))
                await Task.WhenAny(allTasksCompleted, cts.Task);

            return invocations;
        }
    }
}
