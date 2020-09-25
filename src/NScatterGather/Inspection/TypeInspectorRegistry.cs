using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NScatterGather.Inspection
{
    internal class TypeInspectorRegistry
    {
        private static readonly ConcurrentDictionary<Type, TypeInspector> _registry =
            new ConcurrentDictionary<Type, TypeInspector>();

        public static void Register<T>() =>
            Register(typeof(T));

        public static void Register(Type t)
        {
            _ = _registry.TryAdd(t, new TypeInspector(t));
        }

        public static TypeInspector Of<T>() =>
            Of(typeof(T));

        public static TypeInspector Of(Type t)
        {
            _ = _registry.TryGetValue(t, out var inspector);
            return inspector ??
                throw new KeyNotFoundException($"No {nameof(TypeInspector)} for type '{t.Name}' was registered.");
        }

        internal static void Clear() =>
            _registry.Clear();
    }
}
