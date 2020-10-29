using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NScatterGather.Invocations;

namespace NScatterGather
{
    public class AggregatedResponse<TResponse>
    {
        public int TotalInvocationsCount =>
            Completed.Count +
            Faulted.Count +
            Incomplete.Count;

        public IReadOnlyList<CompletedInvocation<TResponse>> Completed { get; }

        public IReadOnlyList<FaultedInvocation> Faulted { get; }

        public IReadOnlyList<IncompleteInvocation> Incomplete { get; }

        internal AggregatedResponse(
            IEnumerable<LiveInvocationHolder<TResponse>> invocations)
        {
            var completed = new List<CompletedInvocation<TResponse>>();
            var faulted = new List<FaultedInvocation>();
            var incomplete = new List<IncompleteInvocation>();

            foreach (var invocation in invocations)
            {
                if (invocation.CompletedSuccessfully)
                    completed.Add(AsCompletedInvocation(invocation));
                else if (invocation.Faulted)
                    faulted.Add(AsFaultedInvocation(invocation));
                else
                    incomplete.Add(AsIncompleteInvocation(invocation));
            }

            Completed = completed.AsReadOnly();
            Faulted = faulted.AsReadOnly();
            Incomplete = incomplete.AsReadOnly();
        }

        private CompletedInvocation<TResponse> AsCompletedInvocation(
            LiveInvocationHolder<TResponse> invocation)
        {
            return new CompletedInvocation<TResponse>(
                invocation.Recipient.Type,
                invocation.Task.Result,
                GetDuration(invocation));
        }

        private FaultedInvocation AsFaultedInvocation(
            LiveInvocationHolder<TResponse> invocation)
        {
            return new FaultedInvocation(
                invocation.Recipient.Type,
                ExtractException(invocation.Task.Exception),
                GetDuration(invocation));

            // Local functions.

            static Exception? ExtractException(Exception? exception)
            {
                if (exception is null) return null;

                if (exception is AggregateException aEx)
                    return ExtractException(aEx.InnerException) ?? aEx;

                if (exception is TargetInvocationException tIEx)
                    return ExtractException(tIEx.InnerException) ?? tIEx;

                return exception;
            }
        }

        private IncompleteInvocation AsIncompleteInvocation(
            LiveInvocationHolder<TResponse> invocation)
        {
            return new IncompleteInvocation(invocation.Recipient.Type);
        }

        private TimeSpan GetDuration(
            LiveInvocationHolder<TResponse> invocation)
        {
            var duration = (invocation.FinishedAt ?? DateTime.UtcNow) - invocation.StartedAt;
            return duration;
        }

        public void Deconstruct(
            out IReadOnlyList<CompletedInvocation<TResponse>> completed,
            out IReadOnlyList<FaultedInvocation> faulted,
            out IReadOnlyList<IncompleteInvocation> incomplete)
        {
            completed = Completed;
            faulted = Faulted;
            incomplete = Incomplete;
        }
    }
}
