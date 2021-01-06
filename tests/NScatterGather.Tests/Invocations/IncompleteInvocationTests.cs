using System;
using Xunit;

namespace NScatterGather.Invocations
{
    public class IncompleteInvocationTests
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

            var invocation = new IncompleteInvocation(expectedDescription);
            var (id, name, type, lifetime, strategy) = invocation.Recipient;

            Assert.Equal(expectedDescription.Id, id);
            Assert.Equal(expectedDescription.Name, name);
            Assert.Equal(expectedDescription.Type, type);
            Assert.Equal(expectedDescription.Lifetime, lifetime);
            Assert.Equal(expectedDescription.CollisionStrategy, strategy);
        }
    }
}
