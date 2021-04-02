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
            return await Send(request, new ScatterGatherOptions(), timeout).ConfigureAwait(false);
        }

        public async Task<AggregatedResponse<object?>> Send(
            object request,
            ScatterGatherOptions options,
            TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource(timeout);
            return await Send(request, options, cts.Token).ConfigureAwait(false);
        }

        public async Task<AggregatedResponse<object?>> Send(
            object request,
            CancellationToken cancellationToken = default)
        {
            return await Send(request, new ScatterGatherOptions(), cancellationToken).ConfigureAwait(false);
        }

        public async Task<AggregatedResponse<object?>> Send(
            object request,
            ScatterGatherOptions options,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var recipients = _scope.ListRecipientsAccepting(request.GetType());

            var runners = await Invoke(recipients, request, options, cancellationToken).ConfigureAwait(false);

            return AggregatedResponseFactory.CreateFrom(runners, options);
        }

        private async Task<IReadOnlyList<RecipientRunner<object?>>> Invoke(
            IReadOnlyList<Recipient> recipients,
            object request,
            ScatterGatherOptions options,
            CancellationToken cancellationToken)
        {
            options = options.Clone(); // Create a snapshot of the options.
            var cancellation = new CancellationGroup(cancellationToken);

            var runners = recipients.SelectMany(recipient => recipient.Accept(request, cancellation.CancellationToken)).ToArray();

            var coordinator = new RunnersCoordinator<object?>(runners, options);
            coordinator.Start(cancellation);

            await coordinator.Completed;

            if (cancellationToken.IsCancellationRequested)
                await WaitForLatecomers(runners, options).ConfigureAwait(false);

            return runners;
        }

        public async Task<AggregatedResponse<TResponse>> Send<TResponse>(
            object request,
            TimeSpan timeout)
        {
            return await Send<TResponse>(request, new ScatterGatherOptions(), timeout).ConfigureAwait(false);
        }

        public async Task<AggregatedResponse<TResponse>> Send<TResponse>(
            object request,
            ScatterGatherOptions options,
            TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource(timeout);
            return await Send<TResponse>(request, options, cts.Token).ConfigureAwait(false);
        }

        public async Task<AggregatedResponse<TResponse>> Send<TResponse>(
            object request,
            CancellationToken cancellationToken = default)
        {
            return await Send<TResponse>(request, new ScatterGatherOptions(), cancellationToken).ConfigureAwait(false);
        }

        public async Task<AggregatedResponse<TResponse>> Send<TResponse>(
            object request,
            ScatterGatherOptions options,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var recipients = _scope.ListRecipientsReplyingWith(request.GetType(), typeof(TResponse));

            var runners = await Invoke<TResponse>(recipients, request, options, cancellationToken).ConfigureAwait(false);

            return AggregatedResponseFactory.CreateFrom(runners, options);
        }

        private async Task<IReadOnlyList<RecipientRunner<TResponse>>> Invoke<TResponse>(
            IReadOnlyList<Recipient> recipients,
            object request,
            ScatterGatherOptions options,
            CancellationToken cancellationToken)
        {
            options = options.Clone(); // Create a snapshot of the options.
            var cancellation = new CancellationGroup(cancellationToken);

            var runners = recipients.SelectMany(recipient => recipient.ReplyWith<TResponse>(request, cancellationToken)).ToArray();

            var coordinator = new RunnersCoordinator<TResponse>(runners, options);
            coordinator.Start(cancellation);

            await coordinator.Completed;

            if (cancellationToken.IsCancellationRequested)
                await WaitForLatecomers(runners, options).ConfigureAwait(false);

            return runners;
        }

        private async Task WaitForLatecomers<TResponse>(
            IReadOnlyList<RecipientRunner<TResponse>> runners,
            ScatterGatherOptions options)
        {
            var incompleteRunners = runners.Where(r => !r.Task.IsCompleted);

            if (!options.AllowCancellationWindowOnAllRecipients)
                incompleteRunners = incompleteRunners.Where(r => r.AcceptedCancellationToken);

            var completionTasks = incompleteRunners.Select(CreateCompletionTask).ToArray();

            if (!completionTasks.Any()) return;

            await Task.WhenAll(completionTasks).ConfigureAwait(false);

            async Task CreateCompletionTask(RecipientRunner<TResponse> runner)
            {
                var wait = Task.Delay(options.CancellationWindow);
                await Task.WhenAny(runner.Task, wait).ConfigureAwait(false);
            }
        }
    }
}
