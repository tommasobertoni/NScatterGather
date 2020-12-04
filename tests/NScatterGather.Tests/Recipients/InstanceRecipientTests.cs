using System;
using System.Threading.Tasks;
using Xunit;

namespace NScatterGather.Recipients
{
    public class InstanceRecipientTests
    {
        class SomeType
        {
            public string EchoAsString(int n) => n.ToString();
        }

        class SomeTypeWithConstructor
        {
            public SomeTypeWithConstructor(int n) { }
        }

        class SomeAsyncType
        {
            public Task<string> EchoAsString(int n) => Task.FromResult(n.ToString());
        }

        class SomeComputingType
        {
            public void Do(int n) { }
        }

        class SomeAsyncComputingType
        {
            public Task Do(int n) => Task.CompletedTask;
        }

        [Fact]
        public void Error_if_instance_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new InstanceRecipient((null as object)!));
        }

        [Fact]
        public void Error_if_type_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new InstanceRecipient((null as Type)!));
        }

        [Fact]
        public void Error_if_request_type_is_null()
        {
            var recipient = new InstanceRecipient(typeof(SomeType));
            Assert.Throws<ArgumentNullException>(() => recipient.CanAccept(null!));
        }

        [Fact]
        public void Recipient_accepts_input_parameter_type()
        {
            var recipient = new InstanceRecipient(typeof(SomeType));
            bool canAccept = recipient.CanAccept(typeof(int));
            Assert.True(canAccept);
        }

        [Fact]
        public void Error_if_no_empty_constructor()
        {
            Assert.Throws<InvalidOperationException>(() => new InstanceRecipient(typeof(SomeTypeWithConstructor)));
        }

        [Fact]
        public void Recipient_can_be_created_from_type()
        {
            _ = new InstanceRecipient(typeof(SomeType));
        }

        [Fact]
        public void Recipient_can_be_created_from_instance()
        {
            _ = new InstanceRecipient(new SomeType());
        }

        [Fact]
        public void Recipient_type_is_visible()
        {
            var type = typeof(SomeType);
            var recipient = new InstanceRecipient(type);
            Assert.Same(type, recipient.Type);
        }

        [Fact]
        public void Recipient_replies_with_response_type()
        {
            var recipient = new InstanceRecipient(typeof(SomeType));
            bool canAccept = recipient.CanReplyWith(typeof(int), typeof(string));
            Assert.True(canAccept);
        }

        [Fact]
        public void Error_if_request_or_response_types_are_null()
        {
            var recipient = new InstanceRecipient(typeof(SomeType));
            Assert.Throws<ArgumentNullException>(() => recipient.CanReplyWith(typeof(int), null!));
            Assert.Throws<ArgumentNullException>(() => recipient.CanReplyWith(null!, typeof(string)));
        }

        [Fact]
        public void Error_if_request_type_not_supported()
        {
            var recipient = new InstanceRecipient(typeof(SomeType));
            Assert.ThrowsAsync<InvalidOperationException>(() => recipient.Accept(Guid.NewGuid()));
        }

        [Fact]
        public async Task Recipient_accepts_request()
        {
            var recipient = new InstanceRecipient(typeof(SomeType));
            var input = 42;
            var response = await recipient.Accept(input);
            Assert.Equal(input.ToString(), response);
        }

        [Fact]
        public async Task Recipient_accepts_request_and_replies_with_task()
        {
            var recipient = new InstanceRecipient(typeof(SomeAsyncType));
            var input = 42;
            var response = await recipient.Accept(input);
            Assert.Equal(input.ToString(), response);
        }

        // aaa

        [Fact]
        public void Error_if_response_type_not_supported()
        {
            var recipient = new InstanceRecipient(typeof(SomeType));
            Assert.ThrowsAsync<InvalidOperationException>(() => recipient.ReplyWith<Guid>(42));
        }

        [Fact]
        public async Task Recipient_replies_with_response()
        {
            var recipient = new InstanceRecipient(typeof(SomeType));
            var input = 42;
            var response = await recipient.ReplyWith<string>(input);
            Assert.Equal(input.ToString(), response);
        }

        [Fact]
        public async Task Recipient_can_reply_with_task()
        {
            var recipient = new InstanceRecipient(typeof(SomeAsyncType));
            var input = 42;
            var response = await recipient.ReplyWith<string>(input);
            Assert.Equal(input.ToString(), response);
        }

        [Fact]
        public void Recipient_must_return_something()
        {
            var recipient = new InstanceRecipient(typeof(SomeComputingType));
            bool accepts = recipient.CanAccept(typeof(int));
            Assert.False(accepts);
        }

        [Fact]
        public void Error_if_void_returning()
        {
            var recipient = new InstanceRecipient(typeof(SomeComputingType));
            Assert.ThrowsAsync<InvalidOperationException>(() => recipient.Accept(42));
        }

        [Fact]
        public void Recipient_must_return_something_async()
        {
            var recipient = new InstanceRecipient(typeof(SomeAsyncComputingType));
            bool accepts = recipient.CanReplyWith(typeof(int), typeof(Task));
            Assert.False(accepts);
        }

        [Fact]
        public void Error_if_returning_task_without_result()
        {
            var recipient = new InstanceRecipient(typeof(SomeAsyncComputingType));
            Assert.ThrowsAsync<InvalidOperationException>(() => recipient.ReplyWith<Task>(42));
        }
    }
}
