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
            string? name)
        {
            if (registry is null)
                throw new ArgumentNullException(nameof(registry));

            if (instance is null)
                throw new ArgumentNullException(nameof(instance));

            var inspector = registry.For(instance.GetType());
            var descriptor = new TypeRecipientDescriptor(inspector);

            var invoker = new InstanceRecipientInvoker(
                inspector,
                new SingletonRecipientFactory(instance));

            return new InstanceRecipient(
                instance,
                descriptor,
                invoker,
                name);
        }

        protected InstanceRecipient(
            object instance,
            IRecipientDescriptor descriptor,
            IRecipientInvoker invoker,
            string? name) : base(instance.GetType(), descriptor, invoker, name, Lifetime.Singleton)
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
            return new InstanceRecipient(_instance, _descriptor, invoker, Name);
        }
    }
}
