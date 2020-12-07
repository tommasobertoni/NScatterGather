using System;

namespace NScatterGather
{
    public class IncompleteInvocation
    {
        public string? RecipientName { get; }

        public Type? RecipientType { get; }

        internal IncompleteInvocation(
            string? recipientName,
            Type? recipientType)
        {
            RecipientName = recipientName;
            RecipientType = recipientType;
        }
    }
}
