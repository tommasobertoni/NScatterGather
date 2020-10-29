using System;

namespace NScatterGather.Invocations
{
    public class IncompleteInvocation
    {
        public Type RecipientType { get; }

        public IncompleteInvocation(
            Type recipientType)
        {
            RecipientType = recipientType;
        }
    }
}
