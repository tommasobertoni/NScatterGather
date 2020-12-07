using System;
using System.Collections.Generic;
using System.Linq;
using NScatterGather.Inspection;
using NScatterGather.Recipients;

namespace NScatterGather
{
    public delegate void ConflictHandler(ConflictException ex);

    public delegate void ErrorHandler(Exception ex);

    public class RecipientsCollection
    {
        private readonly List<Recipient> _recipients = new List<Recipient>();
        private readonly TypeInspectorRegistry _registry;

        public IReadOnlyList<Type> RecipientTypes => _recipients
            .OfType<InstanceRecipient>()
            .Select(x => x.Type)
            .ToArray();

        internal IReadOnlyList<Recipient> Recipients => _recipients.ToArray();

        public event ConflictHandler? OnConflict;

        public event ErrorHandler? OnError;

        public RecipientsCollection()
            : this(new TypeInspectorRegistry()) { }

        internal RecipientsCollection(TypeInspectorRegistry registry)
        {
            _registry = registry;
        }

        public void Add<T>(string? name = null) =>
            Add(typeof(T), name);

        public void Add(Type recipientType, string? name = null)
        {
            if (recipientType is null)
                throw new ArgumentNullException(nameof(recipientType));

            var inspector = _registry.Register(recipientType);
            _recipients.Add(new InstanceRecipient(recipientType, name, inspector));
        }

        public void Add(object instance, string? name = null)
        {
            if (instance is null)
                throw new ArgumentNullException(nameof(instance));

            var inspector = _registry.Register(instance.GetType());
            _recipients.Add(new InstanceRecipient(instance, name, inspector));
        }

        public void Add<TRequest, TResponse>(Func<TRequest, TResponse> @delegate, string? name = null)
        {
            if (@delegate is null)
                throw new ArgumentNullException(nameof(@delegate));

            var recipient = DelegateRecipient.Create(@delegate, name);
            _recipients.Add(recipient);
        }

        internal void Add(Recipient recipient)
        {
            if (recipient is InstanceRecipient ir)
                _ = _registry.Register(ir.Type);

            _recipients.Add(recipient);
        }

        internal IReadOnlyList<Recipient> ListRecipientsAccepting(Type requestType)
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

        internal IReadOnlyList<Recipient> ListRecipientsReplyingWith(Type requestType, Type responseType)
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

        public RecipientsCollection Clone()
        {
            var clone = new RecipientsCollection(_registry);

            foreach (var recipient in _recipients)
                clone.Add(recipient);

            return clone;
        }
    }
}
