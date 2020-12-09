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

        public void Deconstruct(
            out string? recipientName,
            out Type? recipientType)
        {
            recipientName = RecipientName;
            recipientType = RecipientType;
        }
    }
}
