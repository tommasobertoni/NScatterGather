using System;
using System.Collections.Generic;
using System.Linq;

namespace NScatterGather.Recipients.Collection.Scope
{
    internal class RecipientsScope : IRecipientsScope
    {
        public event CollisionHandler? OnCollision;

        public int RecipientsCount => _recipients.Count;

        private readonly List<Recipient> _recipients = new();

        internal void AddRange(IEnumerable<Recipient> recipients) =>
            _recipients.AddRange(recipients);

        public IReadOnlyList<Recipient> ListRecipientsAccepting(Type requestType)
        {
            var validRecipients = _recipients
                .Where(RecipientCanAccept)
                .ToArray();

            return validRecipients;

            // Local functions.

            bool RecipientCanAccept(Recipient recipient)
            {
                try
                {
                    return recipient.CanAccept(requestType);
                }
                catch (CollisionException ex)
                {
                    OnCollision?.Invoke(ex);
                    return false;
                }
            }
        }

        public IReadOnlyList<Recipient> ListRecipientsReplyingWith(Type requestType, Type responseType)
        {
            var validRecipients = _recipients
                .Where(RecipientCanReplyWith)
                .ToArray();

            return validRecipients;

            // Local functions.

            bool RecipientCanReplyWith(Recipient recipient)
            {
                try
                {
                    return recipient.CanReplyWith(requestType, responseType);
                }
                catch (CollisionException ex)
                {
                    OnCollision?.Invoke(ex);
                    return false;
                }
            }
        }
    }
}
