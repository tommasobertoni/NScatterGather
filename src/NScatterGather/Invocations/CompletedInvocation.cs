using System;
using System.Diagnostics.CodeAnalysis;

namespace NScatterGather.Invocations
{
    public class CompletedInvocation<TResponse>
    {
        public Type? RecipientType { get; }

        [AllowNull, MaybeNull]
        public TResponse Result { get; }

        public TimeSpan Duration { get; }

        internal CompletedInvocation(
            Type? recipientType,
            [AllowNull] TResponse result,
            TimeSpan duration)
        {
            RecipientType = recipientType;
            Result = result;
            Duration = duration;
        }

        public void Deconstruct(
            out Type? recipientType,
            [MaybeNull] out TResponse result,
            out TimeSpan duration)
        {
            recipientType = RecipientType;
            result = Result;
            duration = Duration;
        }
    }
}
