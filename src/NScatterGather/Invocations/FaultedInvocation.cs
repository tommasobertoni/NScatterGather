using System;

namespace NScatterGather.Invocations
{
    public class FaultedInvocation
    {
        public Type RecipientType { get; }

        public Exception? Exception { get; }

        public TimeSpan Duration { get; }

        public FaultedInvocation(
            Type recipientType,
            Exception? exception,
            TimeSpan duration)
        {
            RecipientType = recipientType;
            Exception = exception;
            Duration = duration;
        }

        public void Deconstruct(
            out Type recipientType,
            out Exception? exception,
            out TimeSpan duration)
        {
            recipientType = RecipientType;
            exception = Exception;
            duration = Duration;
        }
    }
}
