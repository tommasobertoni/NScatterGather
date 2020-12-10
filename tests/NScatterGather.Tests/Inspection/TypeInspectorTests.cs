using Xunit;

namespace NScatterGather.Inspection
{
    public class TypeInspectorTests
    {
        class SomeType
        {
            public int Do(int n) => n * 2;

            public string Echo(long x) => x.ToString();
        }

        [Fact]
        public void Method_accepting_request_is_found()
        {
            var inspector = new TypeInspector(typeof(SomeType));

            Assert.True(inspector.HasMethodAccepting(typeof(int)));
            Assert.True(inspector.HasMethodAccepting(typeof(long)));

            bool found = inspector.TryGetMethodAccepting(typeof(int), out var method);
            Assert.True(found);
            Assert.Equal(typeof(SomeType).GetMethod(nameof(SomeType.Do)), method);
        }

        [Fact]
        public void Method_returning_response_is_found()
        {
            var inspector = new TypeInspector(typeof(SomeType));

            Assert.True(inspector.HasMethodReturning(typeof(int), typeof(int)));
            Assert.True(inspector.HasMethodReturning(typeof(long), typeof(string)));

            bool found = inspector.TryGetMethodReturning(typeof(int), typeof(int), out var method);
            Assert.True(found);
            Assert.Equal(typeof(SomeType).GetMethod(nameof(SomeType.Do)), method);
        }
    }
}
