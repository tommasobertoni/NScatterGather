using System;

namespace NScatterGather.Invocations
{
    public class IncompleteInvocation
    {
        public Type? RecipientType { get; }

        internal IncompleteInvocation(
            Type? recipientType)
        {
            RecipientType = recipientType;
        }
    }
}
