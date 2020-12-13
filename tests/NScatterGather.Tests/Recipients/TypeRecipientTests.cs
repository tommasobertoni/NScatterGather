using System;
using System.Threading.Tasks;
using Xunit;

namespace NScatterGather.Recipients
{
    public class TypeRecipientTests
    {
        [Fact]
        public void Recipient_can_be_created_from_type()
        {
            _ = TypeRecipient.Create<SomeType>();
        }

        [Fact]
        public void Error_if_no_empty_constructor()
        {
            Assert.Throws<ArgumentException>(() => TypeRecipient.Create<SomeTypeWithConstructor>());
        }

        [Fact]
        public async Task Recipient_accepts_request()
        {
            var recipient = TypeRecipient.Create<SomeType>();
            var input = 42;
            var response = await recipient.Accept(input);
            Assert.Equal(input.ToString(), response);
        }

        [Fact]
        public async Task Recipient_accepts_request_and_replies_with_task()
        {
            var recipient = TypeRecipient.Create<SomeAsyncType>();
            var input = 42;
            var response = await recipient.Accept(input);
            Assert.Equal(input.ToString(), response);
        }

        [Fact]
        public void Error_if_request_type_is_null()
        {
            var recipient = TypeRecipient.Create<SomeType>();
            Assert.Throws<ArgumentNullException>(() => recipient.CanAccept(null!));
        }

        [Fact]
        public void Recipient_accepts_input_parameter_type()
        {
            var recipient = TypeRecipient.Create<SomeType>();
            bool canAccept = recipient.CanAccept(typeof(int));
            Assert.True(canAccept);
        }

        [Fact]
        public void Recipient_type_is_visible()
        {
            var recipient = TypeRecipient.Create<SomeType>();
            Assert.Same(typeof(SomeType), recipient.Type);
        }

        [Fact]
        public void Recipient_replies_with_response_type()
        {
            var recipient = TypeRecipient.Create<SomeType>();
            bool canAccept = recipient.CanReplyWith(typeof(int), typeof(string));
            Assert.True(canAccept);
        }

        [Fact]
        public void Error_if_request_or_response_types_are_null()
        {
            var recipient = TypeRecipient.Create<SomeType>();
            Assert.Throws<ArgumentNullException>(() => recipient.CanReplyWith(typeof(int), null!));
            Assert.Throws<ArgumentNullException>(() => recipient.CanReplyWith(null!, typeof(string)));
        }

        [Fact]
        public void Error_if_request_type_not_supported()
        {
            var recipient = TypeRecipient.Create<SomeType>();
            Assert.ThrowsAsync<InvalidOperationException>(() => recipient.Accept(Guid.NewGuid()));
        }

        [Fact]
        public void Error_if_response_type_not_supported()
        {
            var recipient = TypeRecipient.Create<SomeType>();
            Assert.ThrowsAsync<InvalidOperationException>(() => recipient.ReplyWith<Guid>(42));
        }

        [Fact]
        public async Task Recipient_replies_with_response()
        {
            var recipient = TypeRecipient.Create<SomeType>();
            var input = 42;
            var response = await recipient.ReplyWith<string>(input);
            Assert.Equal(input.ToString(), response);
        }

        [Fact]
        public async Task Recipient_can_reply_with_task()
        {
            var recipient = TypeRecipient.Create<SomeAsyncType>();
            var input = 42;
            var response = await recipient.ReplyWith<string>(input);
            Assert.Equal(input.ToString(), response);
        }

        [Fact]
        public void Recipient_must_return_something()
        {
            var recipient = TypeRecipient.Create<SomeComputingType>();
            bool accepts = recipient.CanAccept(typeof(int));
            Assert.False(accepts);
        }

        [Fact]
        public void Error_if_void_returning()
        {
            var recipient = TypeRecipient.Create<SomeComputingType>();
            Assert.ThrowsAsync<InvalidOperationException>(() => recipient.Accept(42));
        }

        [Fact]
        public void Recipient_must_return_something_async()
        {
            var recipient = TypeRecipient.Create<SomeAsyncComputingType>();
            bool accepts = recipient.CanReplyWith(typeof(int), typeof(Task));
            Assert.False(accepts);
        }

        [Fact]
        public void Error_if_returning_task_without_result()
        {
            var recipient = TypeRecipient.Create<SomeAsyncComputingType>();
            Assert.ThrowsAsync<InvalidOperationException>(() => recipient.ReplyWith<Task>(42));
        }
    }
}
