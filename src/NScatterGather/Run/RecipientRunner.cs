using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using NScatterGather.Recipients;

namespace NScatterGather.Run
{
    internal class RecipientRunner<TResult>
    {
        public Recipient Recipient { get; }

        public bool CompletedSuccessfully { get; private set; }

        [AllowNull]
        public TResult Result { get; private set; }

        public bool Faulted { get; private set; }

        public Exception? Exception { get; set; }

        public DateTime? StartedAt { get; private set; }

        public DateTime? FinishedAt { get; private set; }

        public RecipientRunner(Recipient recipient)
        {
            Recipient = recipient ?? throw new ArgumentNullException(nameof(recipient));
            Result = default;
        }

        public Task Run(Func<Recipient, Task<TResult>> invocation)
        {
            if (invocation is null)
                throw new ArgumentNullException(nameof(invocation));

            if (StartedAt.HasValue)
                throw new InvalidOperationException("Run already executed.");

            StartedAt = DateTime.UtcNow;

            var runnerTask = RunInvocation(invocation);

            UpdateRunnerPropertiesWhenCompleted(runnerTask);

            var tcs = new TaskCompletionSource<bool>();
            runnerTask.ContinueWith(t => tcs.SetResult(!t.IsFaulted));
            return tcs.Task;
        }

        private void UpdateRunnerPropertiesWhenCompleted(
            Task<(TResult, DateTime)> runnerTask)
        {
            var options =
                TaskContinuationOptions.ExecuteSynchronously |
                TaskContinuationOptions.NotOnCanceled;

            runnerTask.ContinueWith(InspectAndExtract, options);

            // Local functions.

            void InspectAndExtract(Task<(TResult, DateTime)> task)
            {
                if (task.IsFaulted)
                {
                    Faulted = true;
                    Exception = ExtractException(task.Exception);
                    FinishedAt = DateTime.UtcNow;
                }
                else if (task.IsCompletedSuccessfully())
                {
                    CompletedSuccessfully = true;
                    var (result, finishedAt) = task.Result;
                    Result = result;
                    FinishedAt = finishedAt;
                }
            }
        }

        private async Task<(TResult, DateTime)> RunInvocation(
            Func<Recipient, Task<TResult>> invocation)
        {
            await Task.Yield();

            var result = await invocation(Recipient);
            var finishedAt = DateTime.UtcNow;

            return (result, finishedAt);
        }

        private Exception? ExtractException(Exception? exception)
        {
            if (exception is null) return null;

            if (exception is AggregateException aEx)
                return ExtractException(aEx.InnerException) ?? aEx;

            if (exception is TargetInvocationException tIEx)
                return ExtractException(tIEx.InnerException) ?? tIEx;

            return exception;
        }
    }
}
