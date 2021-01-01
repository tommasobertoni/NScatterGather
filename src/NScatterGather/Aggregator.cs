using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NScatterGather.Recipients;
using NScatterGather.Recipients.Collection.Scope;
using NScatterGather.Recipients.Run;
using NScatterGather.Responses;

namespace NScatterGather
{
    public class Aggregator
    {
        private readonly IRecipientsScope _scope;

        public Aggregator(RecipientsCollection collection)
        {
            _scope = collection.CreateScope();
        }

        public async Task<AggregatedResponse<object?>> Send(
            object request,
            TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource(timeout);
            return await Send(request, cts.Token).ConfigureAwait(false);
        }

        public async Task<AggregatedResponse<object?>> Send(
            object request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var recipients = _scope.ListRecipientsAccepting(request.GetType());

            var invocations = await Invoke(recipients, request, cancellationToken).ConfigureAwait(false);

            return AggregatedResponseFactory.CreateFrom(invocations);
        }

        private async Task<IReadOnlyList<RecipientRun<object?>>> Invoke(
            IReadOnlyList<Recipient> recipients,
            object request,
            CancellationToken cancellationToken)
        {
            var runners = recipients.SelectMany(recipient => recipient.Accept(request)).ToArray();

            var tasks = runners
                .Select(runner => runner.Start())
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
            return await Send<TResponse>(request, cts.Token).ConfigureAwait(false);
        }

        public async Task<AggregatedResponse<TResponse>> Send<TResponse>(
            object request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var recipients = _scope.ListRecipientsReplyingWith(request.GetType(), typeof(TResponse));

            var runners = await Invoke<TResponse>(recipients, request, cancellationToken).ConfigureAwait(false);

            return AggregatedResponseFactory.CreateFrom(runners);
        }

        private async Task<IReadOnlyList<RecipientRun<TResponse>>> Invoke<TResponse>(
            IReadOnlyList<Recipient> recipients,
            object request,
            CancellationToken cancellationToken)
        {
            var runners = recipients.SelectMany(recipient => recipient.ReplyWith<TResponse>(request)).ToArray();

            var tasks = runners
                .Select(runner => runner.Start())
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
