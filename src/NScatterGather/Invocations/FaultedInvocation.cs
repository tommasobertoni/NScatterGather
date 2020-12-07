using System;

namespace NScatterGather
{
    public class FaultedInvocation
    {
        public string? RecipientName { get; }

        public Type? RecipientType { get; }

        public Exception? Exception { get; }

        public TimeSpan Duration { get; }

        internal FaultedInvocation(
            string? recipientName,
            Type? recipientType,
            Exception? exception,
            TimeSpan duration)
        {
            RecipientName = recipientName;
            RecipientType = recipientType;
            Exception = exception;
            Duration = duration;
        }

        public void Deconstruct(
            out Type? recipientType,
            out Exception? exception,
            out TimeSpan duration)
        {
            recipientType = RecipientType;
            exception = Exception;
            duration = Duration;
        }
    }
}
