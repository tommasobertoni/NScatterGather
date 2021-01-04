using System;

namespace NScatterGather
{
    public class FaultedInvocation
    {
        public Guid RecipientId { get; }

        public string? RecipientName { get; }

        public Type? RecipientType { get; }

        public Exception? Exception { get; }

        public TimeSpan Duration { get; }

        internal FaultedInvocation(
            Guid recipientId,
            string? recipientName,
            Type? recipientType,
            Exception? exception,
            TimeSpan duration)
        {
            RecipientId = recipientId;
            RecipientName = recipientName;
            RecipientType = recipientType;
            Exception = exception;
            Duration = duration;
        }

        public void Deconstruct(
            out Guid recipientId,
            out string? recipientName,
            out Type? recipientType,
            out Exception? exception,
            out TimeSpan duration)
        {
            recipientId = RecipientId;
            recipientName = RecipientName;
            recipientType = RecipientType;
            exception = Exception;
            duration = Duration;
        }
    }
}
