using System;
using Xunit;

namespace NScatterGather.Inspection
{
    [Collection("Singleton registry")]
    public class TypeInspectorRegistryTests : IDisposable
    {
        private readonly TypeInspectorRegistry _registry;

        public TypeInspectorRegistryTests()
        {
            _registry = new TypeInspectorRegistry();
        }

        [Fact]
        public void Can_register_generic_type()
        {
            _registry.For<object>();
        }

        [Fact]
        public void Can_register_type()
        {
            var inspector = _registry.For(typeof(object));
            Assert.NotNull(inspector);
        }

        [Fact]
        public void Inspector_are_cached()
        {
            var inspector1 = _registry.For(typeof(object));
            var inspector2 = _registry.For(typeof(object));
            var inspector3 = _registry.For(typeof(int));
            var inspector4 = _registry.For(typeof(int));

            Assert.Same(inspector1, inspector2);
            Assert.NotSame(inspector2, inspector3);
            Assert.NotEqual(inspector2, inspector3);
            Assert.Same(inspector3, inspector4);
        }

        public void Dispose()
        {
            _registry.Clear();
        }
    }
}
