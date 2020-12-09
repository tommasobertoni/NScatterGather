using System;
using System.Diagnostics.CodeAnalysis;

namespace NScatterGather
{
    public class CompletedInvocation<TResponse>
    {
        public string? RecipientName { get; }

        public Type? RecipientType { get; }

        [AllowNull, MaybeNull]
        public TResponse Result { get; }

        public TimeSpan Duration { get; }

        internal CompletedInvocation(
            string? recipientName,
            Type? recipientType,
            [AllowNull] TResponse result,
            TimeSpan duration)
        {
            RecipientName = recipientName;
            RecipientType = recipientType;
            Result = result;
            Duration = duration;
        }

        public void Deconstruct(
            out string? recipientName,
            out Type? recipientType,
            [MaybeNull] out TResponse result,
            out TimeSpan duration)
        {
            recipientName = RecipientName;
            recipientType = RecipientType;
            result = Result;
            duration = Duration;
        }
    }
}
