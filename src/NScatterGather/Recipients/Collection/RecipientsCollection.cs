using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NScatterGather.Inspection;
using NScatterGather.Recipients;
using NScatterGather.Recipients.Collection.Scope;
using static NScatterGather.CollisionStrategy;
using static NScatterGather.Lifetime;

namespace NScatterGather
{
    public class RecipientsCollection
    {
        public event CollisionHandler? OnCollision;

        public int RecipientsCount => _recipients.Count;

        private readonly List<Recipient> _recipients = new();
        private readonly TypeInspectorRegistry _registry = new TypeInspectorRegistry();
        private readonly CollisionStrategy _defaultCollisionStrategy;

        public RecipientsCollection(CollisionStrategy defaultCollisionStrategy = IgnoreRecipient)
        {
            _defaultCollisionStrategy = defaultCollisionStrategy;
        }

        public Guid Add<TRecipient>(string name) =>
            AddTypeRecipientWithDefaultFactoryMethod<TRecipient>(name: name);

        public Guid Add<TRecipient>(Lifetime lifetime) =>
            AddTypeRecipientWithDefaultFactoryMethod<TRecipient>(lifetime: lifetime);

        public Guid Add<TRecipient>(Func<TRecipient> factoryMethod) =>
            AddTypeRecipient(factoryMethod: factoryMethod);

        public Guid Add<TRecipient>(CollisionStrategy collisionStrategy) =>
            AddTypeRecipientWithDefaultFactoryMethod<TRecipient>(collisionStrategy: collisionStrategy);

        public Guid Add<TRecipient>(
            string? name = null,
            Lifetime lifetime = Transient,
            CollisionStrategy? collisionStrategy = null)
        {
            return AddTypeRecipientWithDefaultFactoryMethod<TRecipient>(name, lifetime, collisionStrategy);
        }

        public Guid Add<TRecipient>(
            Func<TRecipient> factoryMethod,
            string? name = null,
            Lifetime lifetime = Transient,
            CollisionStrategy? collisionStrategy = null)
        {
            return AddTypeRecipient(factoryMethod, name, lifetime, collisionStrategy);
        }

        internal Guid AddTypeRecipientWithDefaultFactoryMethod<TRecipient>(
            string? name = null,
            Lifetime lifetime = Transient,
            CollisionStrategy? collisionStrategy = null)
        {
            if (!HasADefaultConstructor<TRecipient>())
                throw new ArgumentException($"Type '{typeof(TRecipient).Name}' is missing a public, parameterless constructor.");

            static TRecipient factoryMethod() => ((TRecipient)Activator.CreateInstance(typeof(TRecipient)))!;

            return AddTypeRecipient(factoryMethod, name, lifetime, collisionStrategy);

            // Local functions.

            static bool HasADefaultConstructor<T>()
            {
                var defaultContructor = typeof(T).GetConstructor(Type.EmptyTypes);
                return defaultContructor is not null;
            }
        }

        internal Guid AddTypeRecipient<TRecipient>(
            Func<TRecipient> factoryMethod,
            string? name = null,
            Lifetime lifetime = Transient,
            CollisionStrategy? collisionStrategy = null)
        {
            if (factoryMethod is null)
                throw new ArgumentNullException(nameof(factoryMethod));

            var typeRecipient = TypeRecipient.Create(
                _registry, factoryMethod, name, lifetime, collisionStrategy ?? _defaultCollisionStrategy);

            _recipients.Add(typeRecipient);

            return typeRecipient.Id;
        }

        public Guid Add(
            object instance,
            string? name = null,
            CollisionStrategy? collisionStrategy = null)
        {
            if (instance is null)
                throw new ArgumentNullException(nameof(instance));

            var instanceRecipient = InstanceRecipient.Create(
                _registry, instance, name, collisionStrategy ?? _defaultCollisionStrategy);

            _recipients.Add(instanceRecipient);

            return instanceRecipient.Id;
        }

        public Guid Add<TRequest, TResponse>(
            Func<TRequest, TResponse> @delegate,
            string? name = null)
        {
            if (@delegate is null)
                throw new ArgumentNullException(nameof(@delegate));

            var delegateRecipient = DelegateRecipient.Create(@delegate, name);

            _recipients.Add(delegateRecipient);

            return delegateRecipient.Id;
        }

        public Guid Add<TRequest, TResponse>(
            Func<TRequest, CancellationToken, TResponse> @delegate,
            string? name = null)
        {
            if (@delegate is null)
                throw new ArgumentNullException(nameof(@delegate));

            var delegateRecipient = DelegateRecipient.Create(@delegate, name);

            _recipients.Add(delegateRecipient);

            return delegateRecipient.Id;
        }

        internal IRecipientsScope CreateScope()
        {
            var scope = new RecipientsScope();

            var scopedRecipients = _recipients.Select(recipient =>
                recipient.Lifetime == Scoped ? recipient.Clone() : recipient);

            scope.AddRange(scopedRecipients);

            scope.OnCollision += OnCollision;

            return scope;
        }
    }
}
