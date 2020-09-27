using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NScatterGather.Inspection;

namespace NScatterGather.Recipients
{
    public class RecipientsCollection
    {
        private readonly ConcurrentBag<Recipient> _recipients = new ConcurrentBag<Recipient>();
        private readonly ILogger<RecipientsCollection> _logger;
        private readonly TypeInspectorRegistry _registry;

        public RecipientsCollection(
            ILogger<RecipientsCollection> logger)
        {
            _logger = logger;
            _registry = new TypeInspectorRegistry();
        }

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
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, ex.Message);
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
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, ex.Message);
                    return false;
                }
            }
        }
    }
}
