using System;
using System.Threading.Tasks;
using NScatterGather.Recipients;

namespace NScatterGather.Invocations
{
    internal class Invocation<TResult>
    {
        public Recipient Recipient { get; }

        public Task<TResult> Task { get; }

        public Invocation(Recipient recipient, Task<TResult> task)
        {
            Recipient = recipient ?? throw new ArgumentNullException(nameof(recipient));
            Task = task ?? throw new ArgumentNullException(nameof(task));
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
