using System;
using Xunit;

namespace NScatterGather.Recipients
{
    public class InstanceRecipientTests
    {
        [Fact]
        public void Recipient_can_be_created_from_instance()
        {
            _ = InstanceRecipient.Create(new SomeType());
        }

        [Fact]
        public void Error_if_instance_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => InstanceRecipient.Create((null as object)!));
        }
    }
}
