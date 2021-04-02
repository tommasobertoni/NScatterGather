using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NScatterGather.Recipients.Run
{
    internal class RunnersCoordinator<T>
    {
        private readonly IReadOnlyList<RecipientRunner<T>> _runners;
        private readonly ScatterGatherOptions _options;

        private readonly TaskCompletionSource<bool> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly List<RecipientRunner<T>> _completedRunners = new();

        private readonly object _sync = new();

        private CancellationGroup? _cancellation;

        public RunnersCoordinator(
            IReadOnlyList<RecipientRunner<T>> runners,
            ScatterGatherOptions options)
        {
            _runners = runners;
            _options = options;
        }

        public bool HasStarted { get; private set; }

        public Task Completed => _tcs.Task;

        public IReadOnlyList<RecipientRunner<T>> CompletedRunners => _completedRunners;

        public void Start(CancellationGroup? cancellation = null)
        {
            if (HasStarted) return;
            HasStarted = true;

            _cancellation = cancellation;

            if (_cancellation is not null)
                _cancellation.OnCanceled += () => _tcs.TrySetResult(false);

            if (!_runners.Any())
            {
                _tcs.TrySetResult(true);
            }
            else
            {
                foreach (var runner in _runners)
                {
                    var task = runner.Start();
                    task.ContinueWith(_ => OnCompleted(runner));
                }
            }
        }

        private void OnCompleted(RecipientRunner<T> runner)
        {
            // Pre-lock short circuit.
            if (RunWasCanceled()) return;
            if (HasReachedTargetCompletedRunnersCount()) return;

            lock (_sync)
            {
                if (RunWasCanceled()) return;
                if (HasReachedTargetCompletedRunnersCount()) return;

                _completedRunners.Add(runner);

                if (HasReachedTargetCompletedRunnersCount())
                    CancelRunners();

                if (HasReachedTargetCompletedRunnersCount() || _completedRunners.Count == _runners.Count)
                    _tcs.TrySetResult(true);
            }
        }

        private bool RunWasCanceled() =>
            _cancellation?.CancellationToken.IsCancellationRequested ?? false;

        private bool HasReachedTargetCompletedRunnersCount()
        {
            return
                _options.Limit.HasValue &&
                _completedRunners.Count >= _options.Limit.Value;
        }

        private void CancelRunners() =>
            _cancellation?.Cancel();
    }
}
