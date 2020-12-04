using System;
using System.Threading;
using System.Threading.Tasks;

namespace NScatterGather
{
    /// <summary>
    /// Source: AsyncEx
    /// Repo: https://github.com/StephenCleary/AsyncEx
    /// </summary>
    internal sealed class CancellationTokenTaskSource<T> : IDisposable
    {
        private readonly IDisposable? _registration;

        public CancellationTokenTaskSource(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                Task = System.Threading.Tasks.Task.FromCanceled<T>(cancellationToken);
            }
            else
            {
                var tcs = new TaskCompletionSource<T>();

                _registration = cancellationToken.Register(() =>
                    tcs.TrySetCanceled(cancellationToken), useSynchronizationContext: false);

                Task = tcs.Task;
            }
        }

        /// <summary>
        /// Gets the task for the source cancellation token.
        /// </summary>
        public Task<T> Task { get; }

        public void Dispose() =>
            _registration?.Dispose();
    }
}
