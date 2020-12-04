using System;

namespace NScatterGather
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
