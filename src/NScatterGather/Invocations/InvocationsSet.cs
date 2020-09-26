using System.Collections.Generic;

namespace NScatterGather.Invocations
{
    internal class InvocationsSet<TResult>
    {
        public int TotalInvocationsCount =>
            Completed.Count +
            Faulted.Count +
            Incomplete.Count;

        public IReadOnlyList<Invocation<TResult>> Completed => _completed.AsReadOnly();

        public IReadOnlyList<Invocation<TResult>> Faulted => _faulted.AsReadOnly();

        public IReadOnlyList<Invocation<TResult>> Incomplete => _incomplete.AsReadOnly();

        private readonly List<Invocation<TResult>> _completed;
        private readonly List<Invocation<TResult>> _faulted;
        private readonly List<Invocation<TResult>> _incomplete;

        public InvocationsSet()
        {
            _completed = new List<Invocation<TResult>>();
            _faulted = new List<Invocation<TResult>>();
            _incomplete = new List<Invocation<TResult>>();
        }

        public InvocationsSet(IEnumerable<Invocation<TResult>> invocations)
            : this()
        {
            foreach (var invocation in invocations)
                Add(invocation);
        }

        public void Add(Invocation<TResult> invocation)
        {
            if (invocation.Task.IsCompletedSuccessfully)
                _completed.Add(invocation);
            else if (invocation.Task.IsFaulted)
                _faulted.Add(invocation);
            else
                _incomplete.Add(invocation);
        }

        public void Deconstruct(
            out IReadOnlyList<Invocation<TResult>> completed,
            out IReadOnlyList<Invocation<TResult>> faulted,
            out IReadOnlyList<Invocation<TResult>> incomplete)
        {
            completed = Completed;
            faulted = Faulted;
            incomplete = Incomplete;
        }
    }
}
