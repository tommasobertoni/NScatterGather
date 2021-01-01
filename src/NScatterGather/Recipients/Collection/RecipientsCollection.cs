using System;
using System.Collections.Generic;
using System.Linq;
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

        public void Add<TRecipient>(string name) =>
            AddWithDefaultFactoryMethod<TRecipient>(name: name);

        public void Add<TRecipient>(Lifetime lifetime) =>
            AddWithDefaultFactoryMethod<TRecipient>(lifetime: lifetime);

        public void Add<TRecipient>(Func<TRecipient> factoryMethod) =>
            Internal_Add(factoryMethod: factoryMethod);

        public void Add<TRecipient>(CollisionStrategy collisionStrategy) =>
            AddWithDefaultFactoryMethod<TRecipient>(collisionStrategy: collisionStrategy);

        public void Add<TRecipient>(
            string? name = null,
            Lifetime lifetime = Transient,
            CollisionStrategy? collisionStrategy = null)
        {
            AddWithDefaultFactoryMethod<TRecipient>(name, lifetime, collisionStrategy);
        }

        public void Add<TRecipient>(
            Func<TRecipient> factoryMethod,
            string? name = null,
            Lifetime lifetime = Transient,
            CollisionStrategy? collisionStrategy = null)
        {
            Internal_Add(factoryMethod, name, lifetime, collisionStrategy);
        }

        internal void AddWithDefaultFactoryMethod<TRecipient>(
            string? name = null,
            Lifetime lifetime = Transient,
            CollisionStrategy? collisionStrategy = null)
        {
            if (!HasADefaultConstructor<TRecipient>())
                throw new ArgumentException($"Type '{typeof(TRecipient).Name}' is missing a public, parameterless constructor.");

            static TRecipient factoryMethod() => ((TRecipient)Activator.CreateInstance(typeof(TRecipient)))!;

            Internal_Add(factoryMethod, name, lifetime, collisionStrategy);

            // Local functions.

            static bool HasADefaultConstructor<T>()
            {
                var defaultContructor = typeof(T).GetConstructor(Type.EmptyTypes);
                return defaultContructor is not null;
            }
        }

        internal void Internal_Add<TRecipient>(
            Func<TRecipient> factoryMethod,
            string? name = null,
            Lifetime lifetime = Transient,
            CollisionStrategy? collisionStrategy = null)
        {
            if (factoryMethod is null)
                throw new ArgumentNullException(nameof(factoryMethod));

            var typeRecipient = TypeRecipient.Create(
                _registry, factoryMethod, name, lifetime, collisionStrategy ?? _defaultCollisionStrategy);

            Add(typeRecipient);
        }

        public void Add(
            object instance,
            string? name = null,
            CollisionStrategy? collisionStrategy = null)
        {
            if (instance is null)
                throw new ArgumentNullException(nameof(instance));

            var instanceRecipient = InstanceRecipient.Create(
                _registry, instance, name, collisionStrategy ?? _defaultCollisionStrategy);

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
                recipient.Lifetime == Scoped ? recipient.Clone() : recipient);

            scope.AddRange(scopedRecipients);

            scope.OnCollision += OnCollision;

            return scope;
        }
    }
}
