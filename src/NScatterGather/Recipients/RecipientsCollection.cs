using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NScatterGather.Inspection;

namespace NScatterGather.Recipients
{
    public delegate void ConflictHandler(ConflictException ex);

    public delegate void ErrorHandler(Exception ex);

    public class RecipientsCollection
    {
        private readonly ConcurrentBag<Recipient> _recipients = new ConcurrentBag<Recipient>();
        private readonly TypeInspectorRegistry _registry;

        public event ConflictHandler? OnConflict;

        public event ErrorHandler? OnError;

        public RecipientsCollection() =>
            _registry = new TypeInspectorRegistry();

        public void Add<T>() =>
            Add(typeof(T));

        public void Add(Type recipientType)
        {
            if (recipientType is null)
                throw new ArgumentNullException(nameof(recipientType));

            var inspector = _registry.Register(recipientType);
            _recipients.Add(new Recipient(recipientType, inspector));
        }

        public void Add(object instance)
        {
            if (instance is null)
                throw new ArgumentNullException(nameof(instance));

            var inspector = _registry.Register(instance.GetType());
            _recipients.Add(new Recipient(instance, inspector));
        }

        internal IReadOnlyList<Recipient> ListRecipientsAccepting<TRequest>()
        {
            var requestType = typeof(TRequest);

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
                catch (ConflictException ex)
                {
                    OnConflict?.Invoke(ex);
                    return false;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex);
                    return false;
                }
            }
        }

        internal IReadOnlyList<Recipient> ListRecipientsReplyingWith<TRequest, TResponse>()
        {
            var requestType = typeof(TRequest);
            var responseType = typeof(TResponse);

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
                catch (ConflictException ex)
                {
                    OnConflict?.Invoke(ex);
                    return false;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(ex);
                    return false;
                }
            }
        }
    }
}
