using System;
using System.Diagnostics.CodeAnalysis;

namespace NScatterGather
{
    public class CompletedInvocation<TResponse>
    {
        public Guid RecipientId { get; }

        public string? RecipientName { get; }

        public Type? RecipientType { get; }

        [AllowNull, MaybeNull]
        public TResponse Result { get; }

        public TimeSpan Duration { get; }

        internal CompletedInvocation(
            Guid recipientId,
            string? recipientName,
            Type? recipientType,
            [AllowNull] TResponse result,
            TimeSpan duration)
        {
            RecipientId = recipientId;
            RecipientName = recipientName;
            RecipientType = recipientType;
            Result = result;
            Duration = duration;
        }

        public void Deconstruct(
            out Guid recipientId,
            out string? recipientName,
            out Type? recipientType,
            [MaybeNull] out TResponse result,
            out TimeSpan duration)
        {
            recipientId = RecipientId;
            recipientName = RecipientName;
            recipientType = RecipientType;
            result = Result;
            duration = Duration;
        }
    }
}
