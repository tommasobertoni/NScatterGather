using System;
using Xunit;
using static NScatterGather.CollisionStrategy;

namespace NScatterGather.Inspection
{
    public class TypeInspectorTests
    {
        [Fact]
        public void Error_if_type_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new TypeInspector((null as Type)!));
        }

        [Fact]
        public void Method_accepting_request_is_found()
        {
            var inspector = new TypeInspector(typeof(SomeOtherType));

            Assert.True(inspector.HasMethodsAccepting(typeof(int), IgnoreRecipient));
            Assert.True(inspector.HasMethodsAccepting(typeof(long), IgnoreRecipient));

            bool found = inspector.TryGetMethodsAccepting(typeof(int), IgnoreRecipient, out var methods);

            Assert.True(found);
            Assert.Single(methods);
            Assert.Equal(typeof(SomeOtherType).GetMethod(nameof(SomeOtherType.Do)), methods[0]);
        }

        [Fact]
        public void Method_returning_response_is_found()
        {
            var inspector = new TypeInspector(typeof(SomeOtherType));

            Assert.True(inspector.HasMethodsReturning(typeof(int), typeof(int), IgnoreRecipient));
            Assert.True(inspector.HasMethodsReturning(typeof(long), typeof(string), IgnoreRecipient));

            bool found = inspector.TryGetMethodsReturning(typeof(int), typeof(int), IgnoreRecipient, out var methods);

            Assert.True(found);
            Assert.Single(methods);
            Assert.Equal(typeof(SomeOtherType).GetMethod(nameof(SomeOtherType.Do)), methods[0]);
        }

        [Fact]
        public void Error_if_request_type_is_null()
        {
            var inspector = new TypeInspector(typeof(SomeType));
            Assert.Throws<ArgumentNullException>(() => inspector.HasMethodsAccepting((null as Type)!, IgnoreRecipient));
        }

        [Fact]
        public void Error_if_collision()
        {
            var inspector = new TypeInspector(typeof(SomeCollidingType));
            Assert.Throws<CollisionException>(() => inspector.HasMethodsAccepting(typeof(int), IgnoreRecipient));
        }

        [Fact]
        public void Collisions_can_be_allowed()
        {
            var inspector = new TypeInspector(typeof(SomeCollidingType));

            var hasMethodsAccepting = inspector.HasMethodsAccepting(typeof(int), UseAllMethodsMatching);
            Assert.True(hasMethodsAccepting);

            var hasMethodsReturning = inspector.HasMethodsReturning(typeof(int), typeof(string), UseAllMethodsMatching);
            Assert.True(hasMethodsReturning);
        }

        [Fact]
        public void Error_if_invalid_collision_strategy()
        {
            var inspector = new TypeInspector(typeof(SomeType));
            Assert.Throws<ArgumentException>(() => inspector.HasMethodsAccepting(typeof(int), (CollisionStrategy)42));
            Assert.Throws<ArgumentException>(() => inspector.HasMethodsReturning(typeof(int), typeof(string), (CollisionStrategy)42));
        }
    }
}
