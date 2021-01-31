using System;
using Xunit;

namespace NScatterGather.Invocations
{
    public class CompletedInvocationTests
    {
        [Fact]
        public void Can_be_deconstructed()
        {
            var expectedDescription = new RecipientDescription(
                Guid.NewGuid(),
                "My name is",
                typeof(SomeType),
                Lifetime.Singleton,
                CollisionStrategy.UseAllMethodsMatching);

            var expectedResult = 42;
            var expectedDuration = TimeSpan.FromSeconds(0.1);

            var invocation = new CompletedInvocation<int>(
                expectedDescription,
                expectedResult,
                expectedDuration);

            var ((id, name, type, lifetime, strategy), result, duration) = invocation;

            Assert.Equal(expectedDescription.Id, id);
            Assert.Equal(expectedDescription.Name, name);
            Assert.Equal(expectedDescription.Type, type);
            Assert.Equal(expectedDescription.Lifetime, lifetime);
            Assert.Equal(expectedDescription.CollisionStrategy, strategy);
            Assert.Equal(expectedResult, result);
            Assert.Equal(expectedDuration, duration);
        }
    }
}
