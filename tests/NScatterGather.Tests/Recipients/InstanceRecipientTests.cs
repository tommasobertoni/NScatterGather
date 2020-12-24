using System;
using NScatterGather.Inspection;
using Xunit;

namespace NScatterGather.Recipients
{
    public class InstanceRecipientTests
    {
        [Fact]
        public void Recipient_can_be_created_from_instance()
        {
            var registry = new TypeInspectorRegistry();
            _ = InstanceRecipient.Create(registry, new SomeType(), name: null);
        }

        [Fact]
        public void Error_if_registry_is_null()
        {
            Assert.Throws<ArgumentNullException>(() =>
                InstanceRecipient.Create((null as TypeInspectorRegistry)!, new SomeType(), name: null));
        }

        [Fact]
        public void Error_if_instance_is_null()
        {
            var registry = new TypeInspectorRegistry();

            Assert.Throws<ArgumentNullException>(() =>
                InstanceRecipient.Create(registry, (null as object)!, name: null));
        }

        [Fact]
        public void Recipient_has_a_name()
        {
            var registry = new TypeInspectorRegistry();
            var recipient = InstanceRecipient.Create(registry, new SomeType(), name: "My name is");
            Assert.NotNull(recipient.Name);
            Assert.NotEmpty(recipient.Name);
        }

        [Fact]
        public void Can_be_cloned()
        {
            var registry = new TypeInspectorRegistry();
            var recipient = InstanceRecipient.Create(registry, new SomeType(), name: "My name is");
            var clone = recipient.Clone();

            Assert.NotNull(clone);
            Assert.IsType<InstanceRecipient>(clone);
            Assert.Equal(recipient.Name, clone.Name);
            Assert.Equal(recipient.Lifetime, clone.Lifetime);
            Assert.Equal(recipient.Type, (clone as InstanceRecipient).Type);
        }

        [Fact]
        public void Has_expected_lifetime()
        {
            var registry = new TypeInspectorRegistry();
            var recipient = InstanceRecipient.Create(registry, new SomeType(), name: null);
            Assert.Equal(Lifetime.Singleton, recipient.Lifetime);
        }
    }
}
