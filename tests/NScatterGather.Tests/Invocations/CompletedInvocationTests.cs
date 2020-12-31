using System;
using Xunit;

namespace NScatterGather.Invocations
{
    public class CompletedInvocationTests
    {
        [Fact]
        public void Can_be_deconstructed()
        {
            var expectedName = "foo";
            var expectedType = typeof(int);
            var expectedResult = 42;
            var expectedDuration = TimeSpan.FromSeconds(1);

            var invocation = new CompletedInvocation<int>(
                expectedName, expectedType, expectedResult, expectedDuration);

            var (name, type, result, duration) = invocation;

            Assert.Equal(expectedName, name);
            Assert.Equal(expectedType, type);
            Assert.Equal(expectedResult, result);
            Assert.Equal(expectedDuration, duration);
        }
    }
}
