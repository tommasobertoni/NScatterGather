using System;
using System.Collections.Concurrent;

namespace NScatterGather.Inspection
{
    internal class TypeInspectorRegistry
    {
        private readonly ConcurrentDictionary<Type, TypeInspector> _registry =
            new ConcurrentDictionary<Type, TypeInspector>();

        public TypeInspector For<T>() => For(typeof(T));

        public TypeInspector For(Type type)
        {
            if (_registry.TryGetValue(type, out var cached))
                return cached;

            var inspector = new TypeInspector(type);
            _ = _registry.TryAdd(type, inspector);
            return inspector;
        }

        internal void Clear() => _registry.Clear();
    }
}
