using System;
using System.Diagnostics.CodeAnalysis;

namespace NScatterGather
{
    public class CompletedInvocation<TResponse> : Invocation
    {
        [AllowNull, MaybeNull]
        public TResponse Result { get; }

        public TimeSpan Duration { get; }

        internal CompletedInvocation(
            RecipientDescription recipient,
            [AllowNull] TResponse result,
            TimeSpan duration) : base(recipient)
        {
            Result = result;
            Duration = duration;
        }

        public void Deconstruct(
            out RecipientDescription recipient,
            [MaybeNull] out TResponse result,
            out TimeSpan duration)
        {
            recipient = Recipient;
            result = Result;
            duration = Duration;
        }
    }
}
