using System;
using NScatterGather.Inspection;
using Xunit;
using static NScatterGather.CollisionStrategy;

namespace NScatterGather.Recipients.Descriptors
{
    public class TypeRecipientDescriptorTests
    {
        [Fact]
        public void Can_accept_request_type()
        {
            var descriptor = new TypeRecipientDescriptor(new TypeInspector(typeof(SomeNonNullableType)));

            Assert.False(descriptor.CanAccept(typeof(int?), IgnoreRecipient));
            Assert.True(descriptor.CanAccept(typeof(int), IgnoreRecipient));

            var nullableDescriptor = new TypeRecipientDescriptor(new TypeInspector(typeof(SomeNullableType)));

            Assert.True(nullableDescriptor.CanAccept(typeof(int?), IgnoreRecipient));
            Assert.True(nullableDescriptor.CanAccept(typeof(int), IgnoreRecipient));
        }

        [Fact]
        public void Can_reply_with_response_type()
        {
            var descriptor = new TypeRecipientDescriptor(new TypeInspector(typeof(SomeNonNullableType)));

            Assert.False(descriptor.CanReplyWith(typeof(int?), typeof(DateTime?), IgnoreRecipient));
            Assert.True(descriptor.CanReplyWith(typeof(int), typeof(DateTime), IgnoreRecipient));

            var nullableDescriptor = new TypeRecipientDescriptor(new TypeInspector(typeof(SomeNullableType)));

            Assert.True(nullableDescriptor.CanReplyWith(typeof(int?), typeof(DateTime?), IgnoreRecipient));
            Assert.True(nullableDescriptor.CanReplyWith(typeof(int), typeof(DateTime?), IgnoreRecipient));
            Assert.False(nullableDescriptor.CanReplyWith(typeof(int?), typeof(DateTime), IgnoreRecipient));
            Assert.False(nullableDescriptor.CanReplyWith(typeof(int), typeof(DateTime), IgnoreRecipient));
        }
    }
}
