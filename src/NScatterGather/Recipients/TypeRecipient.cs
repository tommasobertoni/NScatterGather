using System;
using NScatterGather.Inspection;
using NScatterGather.Recipients.Descriptors;
using NScatterGather.Recipients.Factories;
using NScatterGather.Recipients.Invokers;

namespace NScatterGather.Recipients
{
    internal class TypeRecipient : Recipient
    {
        public Type Type { get; }

        public static TypeRecipient Create<TRecipient>(
            TypeInspectorRegistry registry,
            Func<TRecipient> factoryMethod,
            string? name,
            Lifetime lifetime,
            CollisionStrategy collisionStrategy)
        {
            if (registry is null)
                throw new ArgumentNullException(nameof(registry));

            if (factoryMethod is null)
                throw new ArgumentNullException(nameof(factoryMethod));

            if (!lifetime.IsValid())
                throw new ArgumentException($"Invalid {nameof(lifetime)} value: {lifetime}");

            if (!collisionStrategy.IsValid())
                throw new ArgumentException($"Invalid {nameof(collisionStrategy)} value: {collisionStrategy}");

            var inspector = registry.For<TRecipient>();

            IRecipientFactory factory = new RecipientFactory(() => factoryMethod()!);

            if (lifetime != Lifetime.Transient)
                factory = new SingletonRecipientFactory(factory);

            return new TypeRecipient(
                typeof(TRecipient),
                new TypeRecipientDescriptor(inspector),
                new InstanceRecipientInvoker(inspector, factory, collisionStrategy),
                name,
                lifetime,
                collisionStrategy);
        }

        protected TypeRecipient(
            Type type,
            IRecipientDescriptor descriptor,
            IRecipientInvoker invoker,
            string? name,
            Lifetime lifetime,
            CollisionStrategy collisionStrategy)
            : base(descriptor, invoker, name, lifetime, collisionStrategy)
        {
            Type = type;
        }

#if NETSTANDARD2_0 || NETSTANDARD2_1
        public override Recipient Clone()
#else
        public override TypeRecipient Clone()
#endif
        {
            var invoker = _invoker.Clone();
            return new TypeRecipient(Type, _descriptor, invoker, Name, Lifetime, CollisionStrategy);
        }
    }
}
