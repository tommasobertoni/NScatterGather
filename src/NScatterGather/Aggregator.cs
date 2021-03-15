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
        public TimeSpan CancellationWindow
        {
            get { return _cancellationWindow; }
            set
            {
                if (value.IsNegative())
                    throw new ArgumentException($"{nameof(CancellationToken)} can't be negative.");

                _cancellationWindow = value;
            }
        }

        public bool AllowCancellationWindowOnAllRecipients { get; set; } = false;

        private TimeSpan _cancellationWindow;
        private readonly IRecipientsScope _scope;

        public Aggregator(RecipientsCollection collection)
        {
            _scope = collection.CreateScope();
            CancellationWindow = TimeSpan.FromMilliseconds(100);
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

        private async Task<IReadOnlyList<RecipientRunner<object?>>> Invoke(
            IReadOnlyList<Recipient> recipients,
            object request,
            CancellationToken cancellationToken)
        {
            var cancellation = new CancellationGroup(cancellationToken);

            var runners = recipients.SelectMany(recipient => recipient.Accept(request, cancellation.CancellationToken)).ToArray();

            var coordinator = new RunnersCoordinator<object?>(runners);
            coordinator.Start(cancellation);

            await coordinator.Completed;

            if (cancellationToken.IsCancellationRequested)
                await WaitForLatecomers(runners).ConfigureAwait(false);

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

        private async Task<IReadOnlyList<RecipientRunner<TResponse>>> Invoke<TResponse>(
            IReadOnlyList<Recipient> recipients,
            object request,
            CancellationToken cancellationToken)
        {
            var cancellation = new CancellationGroup(cancellationToken);

            var runners = recipients.SelectMany(recipient => recipient.ReplyWith<TResponse>(request, cancellationToken)).ToArray();

            var coordinator = new RunnersCoordinator<TResponse>(runners);
            coordinator.Start(cancellation);

            await coordinator.Completed;

            if (cancellationToken.IsCancellationRequested)
                await WaitForLatecomers(runners).ConfigureAwait(false);

            return runners;
        }

        private async Task WaitForLatecomers<TResponse>(IReadOnlyList<RecipientRunner<TResponse>> runners)
        {
            var incompleteRunners = runners.Where(r => !r.Task.IsCompleted);

            if (!AllowCancellationWindowOnAllRecipients)
                incompleteRunners = incompleteRunners.Where(r => r.AcceptedCancellationToken);

            var completionTasks = incompleteRunners.Select(CreateCompletionTask).ToArray();

            if (!completionTasks.Any()) return;

            await Task.WhenAll(completionTasks).ConfigureAwait(false);

            async Task CreateCompletionTask(RecipientRunner<TResponse> runner)
            {
                var wait = Task.Delay(CancellationWindow);
                await Task.WhenAny(runner.Task, wait).ConfigureAwait(false);
            }
        }
    }
}
