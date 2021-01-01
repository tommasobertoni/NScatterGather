using System;
using NScatterGather.Inspection;
using NScatterGather.Recipients.Descriptors;
using NScatterGather.Recipients.Factories;
using NScatterGather.Recipients.Invokers;

namespace NScatterGather.Recipients
{
    internal class InstanceRecipient : TypeRecipient
    {
        private readonly object _instance;

        public static InstanceRecipient Create(
            TypeInspectorRegistry registry,
            object instance,
            string? name,
            CollisionStrategy collisionStrategy)
        {
            if (registry is null)
                throw new ArgumentNullException(nameof(registry));

            if (instance is null)
                throw new ArgumentNullException(nameof(instance));

            if (!collisionStrategy.IsValid())
                throw new ArgumentException($"Invalid {nameof(collisionStrategy)} value: {collisionStrategy}");

            var inspector = registry.For(instance.GetType());
            var descriptor = new TypeRecipientDescriptor(inspector);

            var invoker = new InstanceRecipientInvoker(
                inspector,
                new SingletonRecipientFactory(instance),
                collisionStrategy);

            return new InstanceRecipient(
                instance,
                descriptor,
                invoker,
                name,
                collisionStrategy);
        }

        protected InstanceRecipient(
            object instance,
            IRecipientDescriptor descriptor,
            IRecipientInvoker invoker,
            string? name,
            CollisionStrategy collisionStrategy)
            : base(instance.GetType(), descriptor, invoker, name, Lifetime.Singleton, collisionStrategy)
        {
            _instance = instance;
        }

#if NETSTANDARD2_0 || NETSTANDARD2_1
        public override Recipient Clone()
#else
        public override InstanceRecipient Clone()
#endif
        {
            var invoker = _invoker.Clone();
            return new InstanceRecipient(_instance, _descriptor, invoker, Name, CollisionStrategy);
        }
    }
}
