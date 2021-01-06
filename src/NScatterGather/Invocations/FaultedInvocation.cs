using System;

namespace NScatterGather
{
    public class FaultedInvocation : Invocation
    {
        public Exception? Exception { get; }

        public TimeSpan Duration { get; }

        internal FaultedInvocation(
            RecipientDescription recipient,
            Exception? exception,
            TimeSpan duration) : base(recipient)
        {
            Exception = exception;
            Duration = duration;
        }

        public void Deconstruct(
            out RecipientDescription recipient,
            out Exception? exception,
            out TimeSpan duration)
        {
            recipient = Recipient;
            exception = Exception;
            duration = Duration;
        }
    }
}
