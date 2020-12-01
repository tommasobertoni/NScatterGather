using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NScatterGather.Recipients;
using NScatterGather.Run;

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

            return AggregatedResponseFactory.CreateFrom(invocations);
        }

        private async Task<IReadOnlyList<RecipientRunner<object?>>> Invoke<TRequest>(
            IReadOnlyList<Recipient> recipients,
            TRequest request,
            CancellationToken cancellationToken)
        {
            var runners = recipients.Select(r => new RecipientRunner<object?>(r)).ToArray();

            var tasks = runners
                .Select(runner => runner.Run(x => x.Accept(request)))
                .ToArray();

            var allTasksCompleted = Task.WhenAll(tasks);

            if (allTasksCompleted.IsCompletedSuccessfully())
                return runners;

            using (var cancellation = new CancellationTokenTaskSource<object?[]>(cancellationToken))
                await Task.WhenAny(allTasksCompleted, cancellation.Task).ConfigureAwait(false);

            return runners;
        }

        public async Task<AggregatedResponse<TResponse>> Send<TRequest, TResponse>(
            TRequest request,
            CancellationToken cancellationToken = default)
        {
            var recipients = _recipients.ListRecipientsReplyingWith<TRequest, TResponse>();

            var runners = await Invoke<TRequest, TResponse>(recipients, request, cancellationToken)
                .ConfigureAwait(false);

            return AggregatedResponseFactory.CreateFrom(runners);
        }

        private async Task<IReadOnlyList<RecipientRunner<TResponse>>> Invoke<TRequest, TResponse>(
            IReadOnlyList<Recipient> recipients,
            TRequest request,
            CancellationToken cancellationToken)
        {
            var runners = recipients.Select(r => new RecipientRunner<TResponse>(r)).ToArray();

            var tasks = runners
                .Select(runner => runner.Run(x => x.ReplyWith<TRequest, TResponse>(request)))
                .ToArray();

            var allTasksCompleted = Task.WhenAll(tasks);

            if (allTasksCompleted.IsCompletedSuccessfully())
                return runners;

            using (var cancellation = new CancellationTokenTaskSource<TResponse[]>(cancellationToken))
                await Task.WhenAny(allTasksCompleted, cancellation.Task).ConfigureAwait(false);

            return runners;
        }
    }
}
