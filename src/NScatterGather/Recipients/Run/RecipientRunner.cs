﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using NScatterGather.Recipients.Invokers;
using static System.Threading.Tasks.TaskContinuationOptions;

namespace NScatterGather.Recipients.Run
{
    internal class RecipientRunner<TResult>
    {
        public Recipient Recipient { get; }

        public bool CompletedSuccessfully { get; private set; }

        public bool AcceptedCancellationToken => _preparedInvocation.AcceptedCancellationToken;

        [MaybeNull, AllowNull]
        public TResult Result { get; private set; }

        public Task Task { get; private set; } = Task.CompletedTask;

        public bool Faulted { get; private set; }

        public Exception? Exception { get; set; }

        public DateTime StartedAt { get; private set; }

        public DateTime FinishedAt { get; private set; }

        public TimeSpan Duration => FinishedAt - StartedAt;

        private readonly PreparedInvocation<TResult> _preparedInvocation;

        public RecipientRunner(Recipient recipient, PreparedInvocation<TResult> preparedInvocation)
        {
            Recipient = recipient;
            _preparedInvocation = preparedInvocation;
        }

        public Task Start()
        {
            if (StartedAt != default)
                throw new InvalidOperationException("Run already executed.");

            StartedAt = DateTime.UtcNow;

            var tcs = new TaskCompletionSource<bool>();

            var runnerTask = Task.Run(async () => await _preparedInvocation.Execute().ConfigureAwait(false));

            runnerTask.ContinueWith(completedTask =>
            {
                InspectAndExtract(completedTask);
                tcs.SetResult(!completedTask.IsFaulted);
            }, ExecuteSynchronously | NotOnCanceled);

            // This task won't throw if the invocation failed with an exception.
            Task = tcs.Task;

            return Task;
        }

        private void InspectAndExtract(Task<TResult> task)
        {
            if (task.IsCompleted)
                FinishedAt = DateTime.UtcNow;

            if (task.IsCompletedSuccessfully())
            {
                CompletedSuccessfully = true;
                Result = task.Result;
            }
            else if (task.IsFaulted)
            {
                Faulted = true;

                /*
                 * task.Exception would be null only due to a race condition while accessing
                 * the task.Faulted property and the set of the exception.
                 * Since the task is already complete, it's ensured that the exception is not null.
                 * 
                 * From the source code:
                 *   A "benevolent" race condition makes it possible to return null when IsFaulted is
                 *   true (i.e., if IsFaulted is set just after the check to IsFaulted above).
                 */
                Exception = ExtractException(task.Exception!);
            }
        }

        private Exception? ExtractException(Exception exception)
        {
            if (exception is AggregateException aEx)
                return aEx.InnerExceptions.Count == 1 ? ExtractException(aEx.InnerException!) : aEx;

            if (exception is TargetInvocationException tIEx)
                return tIEx.InnerException is null ? tIEx : ExtractException(tIEx.InnerException);

            return exception;
        }
    }
}
