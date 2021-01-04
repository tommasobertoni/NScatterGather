using System;

namespace NScatterGather
{
    public class IncompleteInvocation
    {
        public Guid RecipientId { get; }

        public string? RecipientName { get; }

        public Type? RecipientType { get; }

        internal IncompleteInvocation(
            Guid recipientId,
            string? recipientName,
            Type? recipientType)
        {
            RecipientId = recipientId;
            RecipientName = recipientName;
            RecipientType = recipientType;
        }

        public void Deconstruct(
            out Guid recipientId,
            out string? recipientName,
            out Type? recipientType)
        {
            recipientId = RecipientId;
            recipientName = RecipientName;
            recipientType = RecipientType;
        }
    }
}
