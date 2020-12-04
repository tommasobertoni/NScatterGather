using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NScatterGather.Recipients;
using NScatterGather.Responses;
using NScatterGather.Run;

namespace NScatterGather
{
    public class Aggregator
    {
        private readonly RecipientsCollection _recipients;

        public Aggregator(RecipientsCollection recipients) =>
            _recipients = recipients;

        public async Task<AggregatedResponse<object?>> Send(
            object request,
            TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource(timeout);
            return await Send(request, cts.Token);
        }

        public async Task<AggregatedResponse<object?>> Send(
            object request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var recipients = _recipients.ListRecipientsAccepting(request.GetType());

            var invocations = await Invoke(recipients, request, cancellationToken)
                .ConfigureAwait(false);

            return AggregatedResponseFactory.CreateFrom(invocations);
        }

        private async Task<IReadOnlyList<RecipientRunner<object?>>> Invoke(
            IReadOnlyList<Recipient> recipients,
            object request,
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

        public async Task<AggregatedResponse<TResponse>> Send<TResponse>(
            object request,
            TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource(timeout);
            return await Send<TResponse>(request, cts.Token);
        }

        public async Task<AggregatedResponse<TResponse>> Send<TResponse>(
            object request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var recipients = _recipients.ListRecipientsReplyingWith(request.GetType(), typeof(TResponse));

            var runners = await Invoke<TResponse>(recipients, request, cancellationToken)
                .ConfigureAwait(false);

            return AggregatedResponseFactory.CreateFrom(runners);
        }

        private async Task<IReadOnlyList<RecipientRunner<TResponse>>> Invoke<TResponse>(
            IReadOnlyList<Recipient> recipients,
            object request,
            CancellationToken cancellationToken)
        {
            var runners = recipients.Select(r => new RecipientRunner<TResponse>(r)).ToArray();

            var tasks = runners
                .Select(runner => runner.Run(x => x.ReplyWith<TResponse>(request)))
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
