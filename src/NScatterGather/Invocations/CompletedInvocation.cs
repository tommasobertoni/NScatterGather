using System;

namespace NScatterGather.Invocations
{
    public class CompletedInvocation<TResponse>
    {
        public Type RecipientType { get; }

        public TResponse Result { get; }

        public TimeSpan Duration { get; }

        public CompletedInvocation(
            Type recipientType,
            TResponse result,
            TimeSpan duration)
        {
            RecipientType = recipientType;
            Result = result;
            Duration = duration;
        }

        public void Deconstruct(
            out Type recipientType,
            out TResponse result,
            out TimeSpan duration)
        {
            recipientType = RecipientType;
            result = Result;
            duration = Duration;
        }
    }
}
