using System;
using System.Threading.Tasks;
using NScatterGather.Recipients;
using Xunit;

namespace NScatterGather.Invocations
{
    public class InvocationTests
    {
        private readonly Recipient _recipient;

        public InvocationTests()
        {
            _recipient = new Recipient(typeof(object));
        }

        [Fact]
        public void Constructor_parameters_are_used()
        {
            var task = Task.FromResult(42);
            var invocation = new LiveInvocationHolder<int>(_recipient, () => task);

            Assert.NotNull(invocation.Recipient);
            Assert.Same(_recipient, invocation.Recipient);
            Assert.NotNull(invocation.Task);
            Assert.Same(task, invocation.Task);
        }

        [Fact]
        public void Error_if_recipient_parameter_is_null()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new LiveInvocationHolder<int>(null!, () => Task.FromResult(42)));
        }

        [Fact]
        public void Can_be_deconstructed()
        {
            var t = Task.FromResult(42);
            var invocation = new LiveInvocationHolder<int>(_recipient, () => t);

            var (recipient, task) = invocation;

            Assert.Same(_recipient, recipient);
            Assert.Same(t, task);
        }
    }
}
