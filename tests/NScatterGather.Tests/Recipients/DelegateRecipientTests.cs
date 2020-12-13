using System;
using Xunit;

namespace NScatterGather.Recipients
{
    public class DelegateRecipientTests
    {
        [Fact]
        public void Error_if_instance_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => DelegateRecipient.Create<int, string>(null!));
        }

        [Fact]
        public void Types_are_parsed_correctly()
        {
            static string? func(int? n) => n?.ToString();
            var recipient = DelegateRecipient.Create<int?, string?>(func);
            Assert.Equal(typeof(int?), recipient.In);
            Assert.Equal(typeof(string), recipient.Out);
        }

        [Fact]
        public void Recipient_has_a_name()
        {
            static string? func(int? n) => n?.ToString();
            var recipient = DelegateRecipient.Create<int?, string?>(func);
            Assert.NotNull(recipient.GetRecipientName());
            Assert.NotEmpty(recipient.GetRecipientName());
        }

        [Fact]
        public void Recipient_can_accept_request_type()
        {
            static string? func(int? n) => n?.ToString();
            var recipient = DelegateRecipient.Create<int?, object?>(func);

            Assert.True(recipient.CanAccept(typeof(int)));
            Assert.True(recipient.CanAccept(typeof(int?)));
            Assert.False(recipient.CanAccept(typeof(object)));
            Assert.False(recipient.CanAccept(typeof(string)));
        }

        [Fact]
        public void Recipient_can_reply_with_response_type()
        {
            static int? func(int? n) => n;
            var recipient = DelegateRecipient.Create<int?, int?>(func);

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
            var recipient = DelegateRecipient.Create<int?, string?>(func);

            Assert.Throws<InvalidOperationException>(() => recipient.Invoke(DateTime.UtcNow));
        }

        [Fact]
        public void Error_if_it_does_not_reply_with_the_response_type()
        {
            static string? func(int? n) => n?.ToString();
            var recipient = DelegateRecipient.Create<int?, string?>(func);

            Assert.Throws<InvalidOperationException>(() => recipient.Invoke<DateTime>(42));
        }

        [Fact]
        public void Invokes_delegate_with_matching_request_type()
        {
            bool invoked = false;

            string? func(int? n)
            {
                invoked = true;
                return n?.ToString();
            }

            var recipient = DelegateRecipient.Create<int?, string?>(func);

            var input = 42;
            var result = recipient.Invoke(input);

            Assert.True(invoked);
            Assert.Equal(input.ToString(), result);
        }

        [Fact]
        public void Invokes_delegate_with_matching_response_type()
        {
            bool invoked = false;

            string? func(int? n)
            {
                invoked = true;
                return n?.ToString();
            }

            var recipient = DelegateRecipient.Create<int?, string?>(func);

            var input = 42;
            var result = recipient.Invoke<string>(input);

            Assert.True(invoked);
            Assert.Equal(input.ToString(), result);
        }
    }
}
