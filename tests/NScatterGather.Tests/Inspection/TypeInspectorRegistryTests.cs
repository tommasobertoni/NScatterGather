using System;
using System.Collections.Generic;
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
            _registry.Register<object>();
        }

        [Fact]
        public void Can_register_type()
        {
            _registry.Register(typeof(object));
        }

        [Fact]
        public void Registered_type_inspector_can_be_found()
        {
            _registry.Register<object>();

            Assert.NotNull(_registry.Of<object>());
            Assert.NotNull(_registry.Of(typeof(object)));
        }

        [Fact]
        public void Error_if_type_was_never_registered()
        {
            Assert.Throws<KeyNotFoundException>(() => _registry.Of<object>());
            Assert.Throws<KeyNotFoundException>(() => _registry.Of(typeof(object)));
        }

        public void Dispose()
        {
            _registry.Clear();
        }
    }
}
