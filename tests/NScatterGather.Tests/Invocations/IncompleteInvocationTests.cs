using Xunit;

namespace NScatterGather.Invocations
{
    public class IncompleteInvocationTests
    {
        [Fact]
        public void Can_be_deconstructed()
        {
            var expectedName = "foo";
            var expectedType = typeof(int);

            var invocation = new IncompleteInvocation(expectedName, expectedType);
            var (name, type) = invocation;

            Assert.Equal(expectedName, name);
            Assert.Equal(expectedType, type);
        }
    }
}
