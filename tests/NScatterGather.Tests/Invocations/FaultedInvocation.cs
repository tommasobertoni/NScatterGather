using System;
using Xunit;

namespace NScatterGather.Invocations
{
    public class FaultedInvocationTests
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

            var expectedException = new Exception();
            var expectedDuration = TimeSpan.FromSeconds(0.1);

            var invocation = new FaultedInvocation(
                expectedDescription,
                expectedException,
                expectedDuration);

            var ((id, name, type, lifetime, strategy), exception, duration) = invocation;

            Assert.Equal(expectedDescription.Id, id);
            Assert.Equal(expectedDescription.Name, name);
            Assert.Equal(expectedDescription.Type, type);
            Assert.Equal(expectedDescription.Lifetime, lifetime);
            Assert.Equal(expectedDescription.CollisionStrategy, strategy);
            Assert.Equal(expectedException, exception);
            Assert.Equal(expectedDuration, duration);
        }
    }
}
