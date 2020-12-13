﻿using Xunit;

namespace NScatterGather.Inspection
{
    public class TypeInspectorTests
    {
        [Fact]
        public void Method_accepting_request_is_found()
        {
            var inspector = new TypeInspector(typeof(SomeOtherType));

            Assert.True(inspector.HasMethodAccepting(typeof(int)));
            Assert.True(inspector.HasMethodAccepting(typeof(long)));

            bool found = inspector.TryGetMethodAccepting(typeof(int), out var method);
            Assert.True(found);
            Assert.Equal(typeof(SomeOtherType).GetMethod(nameof(SomeOtherType.Do)), method);
        }

        [Fact]
        public void Method_returning_response_is_found()
        {
            var inspector = new TypeInspector(typeof(SomeOtherType));

            Assert.True(inspector.HasMethodReturning(typeof(int), typeof(int)));
            Assert.True(inspector.HasMethodReturning(typeof(long), typeof(string)));

            bool found = inspector.TryGetMethodReturning(typeof(int), typeof(int), out var method);
            Assert.True(found);
            Assert.Equal(typeof(SomeOtherType).GetMethod(nameof(SomeOtherType.Do)), method);
        }
    }
}
