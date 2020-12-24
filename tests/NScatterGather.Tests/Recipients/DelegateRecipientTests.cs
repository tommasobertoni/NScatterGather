using System;
using System.Threading.Tasks;
using Xunit;

namespace NScatterGather.Recipients
{
    public class DelegateRecipientTests
    {
        [Fact]
        public void Error_if_instance_is_null()
        {
            Assert.Throws<ArgumentNullException>(() =>
                DelegateRecipient.Create<int, string>(null!, name: null));
        }

        [Fact]
        public void Types_are_parsed_correctly()
        {
            static string? func(int? n) => n?.ToString();
            var recipient = DelegateRecipient.Create<int?, string?>(func, name: null);
            Assert.Equal(typeof(int?), recipient.RequestType);
            Assert.Equal(typeof(string), recipient.ResponseType);
        }

        [Fact]
        public void Recipient_has_a_name()
        {
            static string? func(int? n) => n?.ToString();
            var recipient = DelegateRecipient.Create<int?, string?>(func, name: "My name is");
            Assert.NotNull(recipient.Name);
            Assert.NotEmpty(recipient.Name);
        }

        [Fact]
        public void Recipient_can_accept_request_type()
        {
            static string? func(int? n) => n?.ToString();
            var recipient = DelegateRecipient.Create<int?, object?>(func, name: null);

            Assert.True(recipient.CanAccept(typeof(int)));
            Assert.True(recipient.CanAccept(typeof(int?)));
            Assert.False(recipient.CanAccept(typeof(object)));
            Assert.False(recipient.CanAccept(typeof(string)));
        }

        [Fact]
        public void Recipient_can_reply_with_response_type()
        {
            static int? func(int? n) => n;
            var recipient = DelegateRecipient.Create<int?, int?>(func, name: null);

            Assert.True(recipient.CanReplyWith(typeof(int), typeof(int)));
            Assert.True(recipient.CanReplyWith(typeof(int?), typeof(int)));
            Assert.True(recipient.CanReplyWith(typeof(int), typeof(int?)));
            Assert.True(recipient.CanReplyWith(typeof(int?), typeof(int?)));
            Assert.False(recipient.CanReplyWith(typeof(int?), typeof(object)));
            Assert.False(recipient.CanReplyWith(typeof(int?), typeof(string)));
        }

        [Fact]
        public void Error_if_it_does_not_accept_the_request_type()
        {
            static string? func(int? n) => n?.ToString();
            var recipient = DelegateRecipient.Create<int?, string?>(func, name: null);

            Assert.Throws<InvalidOperationException>(() => recipient.Accept(DateTime.UtcNow));
        }

        [Fact]
        public void Error_if_it_does_not_reply_with_the_response_type()
        {
            static string? func(int? n) => n?.ToString();
            var recipient = DelegateRecipient.Create<int?, string?>(func, name: null);

            Assert.Throws<InvalidOperationException>(() => recipient.ReplyWith<DateTime>(42));
        }

        [Fact]
        public async Task Invokes_delegate_with_matching_request_type()
        {
            bool invoked = false;

            string? func(int? n)
            {
                invoked = true;
                return n?.ToString();
            }

            var recipient = DelegateRecipient.Create<int?, string?>(func, name: null);

            var input = 42;
            var runner = recipient.Accept(input);
            await runner.Start();

            var result = runner.Result;

            Assert.True(invoked);
            Assert.Equal(input.ToString(), result);
        }

        [Fact]
        public async Task Invokes_delegate_with_matching_response_type()
        {
            bool invoked = false;

            string? func(int? n)
            {
                invoked = true;
                return n?.ToString();
            }

            var recipient = DelegateRecipient.Create<int?, string?>(func, name: null);

            var input = 42;
            var runner = recipient.ReplyWith<string>(input);
            await runner.Start();

            var result = runner.Result;

            Assert.True(invoked);
            Assert.Equal(input.ToString(), result);
        }

        [Fact]
        public void Can_be_cloned()
        {
            static string? func(int? n) => n?.ToString();
            var recipient = DelegateRecipient.Create<int?, string?>(func, name: "My name is");
            var clone = recipient.Clone();

            Assert.NotNull(clone);
            Assert.IsType<DelegateRecipient>(clone);
            Assert.Equal(recipient.Name, clone.Name);
            Assert.Equal(recipient.Lifetime, clone.Lifetime);
            Assert.Equal(recipient.RequestType, (clone as DelegateRecipient).RequestType);
            Assert.Equal(recipient.ResponseType, (clone as DelegateRecipient).ResponseType);
        }

        [Fact]
        public void Has_expected_lifetime()
        {
            static string? func(int? n) => n?.ToString();
            var recipient = DelegateRecipient.Create<int?, string?>(func, name: null);
            Assert.Equal(Lifetime.Singleton, recipient.Lifetime);
        }
    }
}
