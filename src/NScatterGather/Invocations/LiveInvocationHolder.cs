using System;
using System.Threading.Tasks;
using NScatterGather.Recipients;

namespace NScatterGather.Invocations
{
    internal class LiveInvocationHolder<TResult>
    {
        public Recipient Recipient { get; }

        public Task<TResult> Task { get; }

        public bool CompletedSuccessfully => Task.IsCompletedSuccessfully();

        public bool Faulted => Task.IsFaulted;

        public DateTime StartedAt { get; }

        public DateTime? FinishedAt { get; private set; }

        public LiveInvocationHolder(Recipient recipient, Func<Task<TResult>> invocation)
        {
            Recipient = recipient ?? throw new ArgumentNullException(nameof(recipient));
            
            StartedAt = DateTime.UtcNow;

            Task = invocation();

            Task.ContinueWith(
                _ => FinishedAt = DateTime.UtcNow,
                TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public void Deconstruct(
            out Recipient recipient,
            out Task<TResult> task)
        {
            recipient = Recipient;
            task = Task;
        }
    }
}
