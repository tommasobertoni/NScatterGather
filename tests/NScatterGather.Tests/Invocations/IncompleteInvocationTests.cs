using System;
using Xunit;

namespace NScatterGather.Invocations
{
    public class IncompleteInvocationTests
    {
        [Fact]
        public void Can_be_deconstructed()
        {
            var expectedId = Guid.NewGuid();
            var expectedName = "foo";
            var expectedType = typeof(int);

            var invocation = new IncompleteInvocation(expectedId, expectedName, expectedType);
            var (id, name, type) = invocation;

            Assert.Equal(expectedId, id);
            Assert.Equal(expectedName, name);
            Assert.Equal(expectedType, type);
        }
    }
}
