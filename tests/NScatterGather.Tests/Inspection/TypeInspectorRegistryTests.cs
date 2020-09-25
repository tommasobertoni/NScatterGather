using System;
using System.Collections.Generic;
using Xunit;

namespace NScatterGather.Inspection
{
    public class TypeInspectorRegistryTests : IDisposable
    {
        [Fact]
        public void Can_register_generic_type()
        {
            TypeInspectorRegistry.Register<object>();
        }

        [Fact]
        public void Can_register_type()
        {
            TypeInspectorRegistry.Register(typeof(object));
        }

        [Fact]
        public void Registered_type_inspector_can_be_found()
        {
            TypeInspectorRegistry.Register<object>();

            Assert.NotNull(TypeInspectorRegistry.Of<object>());
            Assert.NotNull(TypeInspectorRegistry.Of(typeof(object)));
        }

        [Fact]
        public void Error_if_type_was_never_registered()
        {
            Assert.Throws<KeyNotFoundException>(() => TypeInspectorRegistry.Of<object>());
            Assert.Throws<KeyNotFoundException>(() => TypeInspectorRegistry.Of(typeof(object)));
        }

        public void Dispose()
        {
            TypeInspectorRegistry.Clear();
        }
    }
}
