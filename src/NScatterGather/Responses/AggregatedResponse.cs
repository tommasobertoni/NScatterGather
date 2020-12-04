using System.Collections.Generic;

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
