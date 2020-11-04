using System;
using System.Collections.Generic;
using System.Reflection;
using NScatterGather.Invocations;
using NScatterGather.Run;

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

            Completed = completed.AsReadOnly();
            Faulted = faulted.AsReadOnly();
            Incomplete = incomplete.AsReadOnly();
        }

        private TimeSpan GetDuration(
            RecipientRunner<TResponse> invocation)
        {
            if (invocation.StartedAt.HasValue && invocation.FinishedAt.HasValue)
                return invocation.FinishedAt.Value - invocation.StartedAt.Value;

            return default;
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
