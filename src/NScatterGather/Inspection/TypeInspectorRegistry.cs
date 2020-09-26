using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NScatterGather.Inspection
{
    internal class TypeInspectorRegistry
    {
        private readonly ConcurrentDictionary<Type, TypeInspector> _registry =
            new ConcurrentDictionary<Type, TypeInspector>();

        public TypeInspector Register<T>() =>
            Register(typeof(T));

        public TypeInspector Register(Type type)
        {
            if (_registry.TryGetValue(type, out var cached))
                return cached;

            var inspector = new TypeInspector(type);
            _ = _registry.TryAdd(type, inspector);
            return inspector;
        }

        public TypeInspector Of<T>() =>
            Of(typeof(T));

        public TypeInspector Of(Type t)
        {
            _ = _registry.TryGetValue(t, out var inspector);
            return inspector ??
                throw new KeyNotFoundException($"No {nameof(TypeInspector)} for type '{t.Name}' was registered.");
        }

        internal void Clear() =>
            _registry.Clear();
    }
}
