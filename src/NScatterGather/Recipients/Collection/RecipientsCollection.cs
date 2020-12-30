using System;
using System.Collections.Generic;
using System.Linq;
using NScatterGather.Inspection;
using NScatterGather.Recipients;
using NScatterGather.Recipients.Collection.Scope;

namespace NScatterGather
{
    public class RecipientsCollection
    {
        public event CollisionHandler? OnCollision;

        public int RecipientsCount => _recipients.Count;

        private readonly List<Recipient> _recipients = new();
        private readonly TypeInspectorRegistry _registry = new TypeInspectorRegistry();

        public void Add<TRecipient>(
            string? name = null,
            Lifetime lifetime = Lifetime.Transient)
        {
            if (!HasADefaultConstructor<TRecipient>())
                throw new ArgumentException($"Type '{typeof(TRecipient).Name}' is missing a public, parameterless constructor.");

            Add(() => ((TRecipient)Activator.CreateInstance(typeof(TRecipient)))!, name, lifetime);

            // Local functions.

            static bool HasADefaultConstructor<T>()
            {
                var defaultContructor = typeof(T).GetConstructor(Type.EmptyTypes);
                return defaultContructor is not null;
            }
        }

        public void Add<TRecipient>(
            Func<TRecipient> factoryMethod,
            string? name = null,
            Lifetime lifetime = Lifetime.Transient)
        {
            if (factoryMethod is null)
                throw new ArgumentNullException(nameof(factoryMethod));

            var typeRecipient = TypeRecipient.Create(_registry, factoryMethod, name, lifetime);

            Add(typeRecipient);
        }

        public void Add(
            object instance,
            string? name = null)
        {
            if (instance is null)
                throw new ArgumentNullException(nameof(instance));

            var instanceRecipient = InstanceRecipient.Create(_registry, instance, name);

            Add(instanceRecipient);
        }

        public void Add<TRequest, TResponse>(
            Func<TRequest, TResponse> @delegate,
            string? name = null)
        {
            if (@delegate is null)
                throw new ArgumentNullException(nameof(@delegate));

            var delegateRecipient = DelegateRecipient.Create(@delegate, name);

            Add(delegateRecipient);
        }

        private void Add(Recipient recipient)
        {
            _recipients.Add(recipient);
        }

        internal IRecipientsScope CreateScope()
        {
            var scope = new RecipientsScope();

            var scopedRecipients = _recipients.Select(recipient =>
            {
                return recipient.Lifetime == Lifetime.Scoped
                    ? recipient.Clone()
                    : recipient;
            });

            scope.AddRange(scopedRecipients);

            scope.OnCollision += OnCollision;

            return scope;
        }
    }
}
