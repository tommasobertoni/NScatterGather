using System.Threading;
using System.Threading.Tasks;
using NScatterGather.Recipients.Descriptors;
using Xunit;

namespace NScatterGather.Recipients.Invokers
{
    public class DelegateRecipientInvokerTests
    {
        [Fact]
        public void Can_be_cloned()
        {
            var descriptor = new DelegateRecipientDescriptor(typeof(int), typeof(string));
            static object? @delegate(object o) { return o.ToString(); }

            var invoker = new DelegateRecipientInvoker(descriptor, @delegate);
            var clone = invoker.Clone();

            Assert.NotNull(clone);
            Assert.IsType<DelegateRecipientInvoker>(invoker);
        }

        [Fact]
        public void Can_be_created_with_cancellation_token()
        {
            var descriptor = new DelegateRecipientDescriptor(typeof(int), typeof(string));
            static object? @delegate(object o, CancellationToken cancellationToken) { return o.ToString(); }
            _ = new DelegateRecipientInvoker(descriptor, @delegate);
        }

        [Fact]
        public async Task Cancellation_token_is_forwarded()
        {
            var descriptor = new DelegateRecipientDescriptor(typeof(int), typeof(string));

            static object? @delegate(object o, CancellationToken cancellationToken)
            {
                return cancellationToken.IsCancellationRequested ? null : o.ToString();
            }

            var invoker = new DelegateRecipientInvoker(descriptor, @delegate);

            {
                var invocations = invoker.PrepareInvocations(42);
                var invocation = invocations[0];
                var result = await invocation.Execute();
                Assert.NotNull(result);
            }

            {
                var invocations = invoker.PrepareInvocations(42, new CancellationToken(canceled: false));
                var invocation = invocations[0];
                var result = await invocation.Execute();
                Assert.NotNull(result);
            }

            {
                var invocations = invoker.PrepareInvocations(42, new CancellationToken(canceled: true));
                var invocation = invocations[0];
                var result = await invocation.Execute();
                Assert.Null(result);
            }
        }
    }
}
