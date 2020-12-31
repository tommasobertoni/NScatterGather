﻿using System;
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
            Lifetime lifetime)
        {
            if (registry is null)
                throw new ArgumentNullException(nameof(registry));

            if (factoryMethod is null)
                throw new ArgumentNullException(nameof(factoryMethod));

            if (!lifetime.IsValid())
                throw new ArgumentException($"Invalid {nameof(lifetime)} value: {lifetime}");

            var inspector = registry.For<TRecipient>();

            IRecipientFactory factory = new RecipientFactory(() => factoryMethod()!);

            if (lifetime != Lifetime.Transient)
                factory = new SingletonRecipientFactory(factory);

            return new TypeRecipient(
                typeof(TRecipient),
                new TypeRecipientDescriptor(inspector),
                new InstanceRecipientInvoker(inspector, factory),
                name,
                lifetime);
        }

        protected TypeRecipient(
            Type type,
            IRecipientDescriptor descriptor,
            IRecipientInvoker invoker,
            string? name,
            Lifetime lifetime) : base(descriptor, invoker, name, lifetime)
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
            return new TypeRecipient(Type, _descriptor, invoker, Name, Lifetime);
        }
    }
}
