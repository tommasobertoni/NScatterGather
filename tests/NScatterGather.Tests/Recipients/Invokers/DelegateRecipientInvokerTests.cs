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
    }
}
