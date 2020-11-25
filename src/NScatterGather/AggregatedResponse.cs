using System;
using System.Collections.Generic;
using NScatterGather.Invocations;
using NScatterGather.Run;

namespace NScatterGather
{
    internal class AggregatedResponseFactory
    {
        public static AggregatedResponse<TResponse> CreateFrom<TResponse>(
            IEnumerable<RecipientRunner<TResponse>> invocations)
        {
            var completed = new List<CompletedInvocation<TResponse>>();
            var faulted = new List<FaultedInvocation>();
            var incomplete = new List<IncompleteInvocation>();

            foreach (var invocation in invocations)
            {
                if (invocation.CompletedSuccessfully)
                {
                    var completedInvocation = new CompletedInvocation<TResponse>(
                        invocation.Recipient.Type,
                        invocation.Result,
                        GetDuration(invocation));

                    completed.Add(completedInvocation);
                }
                else if (invocation.Faulted)
                {
                    var faultedInvocation = new FaultedInvocation(
                        invocation.Recipient.Type,
                        invocation.Exception,
                        GetDuration(invocation));

                    faulted.Add(faultedInvocation);
                }
                else
                    incomplete.Add(new IncompleteInvocation(invocation.Recipient.Type));
            }

            return new AggregatedResponse<TResponse>(completed, faulted, incomplete);
        }

        private static TimeSpan GetDuration<TResponse>(
            RecipientRunner<TResponse> invocation)
        {
            if (invocation.StartedAt.HasValue && invocation.FinishedAt.HasValue)
                return invocation.FinishedAt.Value - invocation.StartedAt.Value;

            return default;
        }
    }

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
            IReadOnlyList<CompletedInvocation<TResponse>> completed,
            IReadOnlyList<FaultedInvocation> faulted,
            IReadOnlyList<IncompleteInvocation> incomplete)
        {
            Completed = completed;
            Faulted = faulted;
            Incomplete = incomplete;
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
