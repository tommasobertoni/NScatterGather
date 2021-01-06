using NScatterGather.Recipients;

namespace NScatterGather.Invocations
{
    internal class RecipientDescriptionFactory
    {
        public static RecipientDescription CreateFrom(Recipient recipient)
        {
            var recipientType = recipient is TypeRecipient ir ? ir.Type : null;

            var description = new RecipientDescription(
                recipient.Id,
                recipient.Name,
                recipientType,
                recipient.Lifetime,
                recipient.CollisionStrategy);

            return description;
        }
    }
}
