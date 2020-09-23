using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace NScatterGather.Inspection
{
    internal class TypeInspector
    {
        private static readonly BindingFlags DefaultFlags = BindingFlags.Public | BindingFlags.Instance;
        private static readonly MethodAnalyzer _methodAnalyzer = new MethodAnalyzer();

        private readonly TypeInspectionCache _inspectionsCache = new TypeInspectionCache();

        private readonly IReadOnlyList<MethodInspection> _methodInspections;
        private readonly Type _type;

        internal TypeInspector(Type type)
        {
            _type = type;
            _methodInspections = InspectMethods(type);

            // Local functions.

            static IReadOnlyList<MethodInspection> InspectMethods(Type type)
            {
                var methods = type.GetMethods(DefaultFlags);
                return methods
                    .Select(method => new MethodInspection(type, method))
                    .ToArray();
            }
        }

        #region Request only

        public bool HasMethodAccepting<TRequest>() =>
            HasMethodAccepting(typeof(TRequest));

        public bool HasMethodAccepting(Type requestType) =>
            TryGetMethodAccepting(requestType, out _);

        public bool TryGetMethodAccepting<TRequest>(
            [MaybeNullWhen(false)] out MethodInfo method) =>
            TryGetMethodAccepting(typeof(TRequest), out method);

        public bool TryGetMethodAccepting(
            Type requestType,
            [MaybeNullWhen(false)] out MethodInfo method)
        {
            // Check in the cache if the analysis was already done.
            if (_inspectionsCache.TryFindInspectionResult(requestType, out var cached))
            {
                // Compliance with the request type is already known.
                method = cached.Method!;
                return cached.IsMatch;
            }

            // Analyze the methods and find a match.

            var matches = ListMatchingMethods(requestType);

            if (matches.Count == 1)
            {
                method = matches.Single();
                _inspectionsCache.TryAdd(requestType, new TypeInspection(true, method));
                return true;
            }

            // Single match not found.

            method = null;
            _inspectionsCache.TryAdd(requestType, new TypeInspection(false, method));

            if (matches.Count > 1)
                throw new InvalidOperationException(
                    $"Type '{_type.Name}' has too many matching methods " +
                    $"accepting '{requestType.Name}'.");

            return false;
        }

        private IReadOnlyList<MethodInfo> ListMatchingMethods(Type requestType)
        {
            return _methodInspections
                .Select(i =>
                {
                    var isMatch = _methodAnalyzer.IsMatch(i, requestType, out var match);
                    return (isMatch, match);
                })
                .Where(x => x.isMatch)
                .Select(x => x.match!)
                .ToArray();
        }

        #endregion

        #region Request and response

        public bool HasMethodReturning<TRequest, TResponse>() =>
            HasMethodReturning(typeof(TRequest), typeof(TResponse));

        public bool HasMethodReturning(Type requestType, Type responseType) =>
            TryGetMethodReturning(requestType, responseType, out _);

        public bool TryGetMethodReturning<TRequest, TResponse>(
            [MaybeNullWhen(false)] out MethodInfo method) =>
            TryGetMethodReturning(typeof(TRequest), typeof(TResponse), out method);

        public bool TryGetMethodReturning(
            Type requestType,
            Type responseType,
            [MaybeNullWhen(false)] out MethodInfo method)
        {
            // Check in the cache if the analysis was already done.
            if (_inspectionsCache.TryFindInspectionResult(requestType, responseType, out var cached))
            {
                // Compliance with the request type is already known.
                method = cached.Method!;
                return cached.IsMatch;
            }

            // Analyze the methods and find a match.

            var matches = ListMatchingMethods(requestType, responseType);

            if (matches.Count == 1)
            {
                method = matches.Single();
                _inspectionsCache.TryAdd(requestType, responseType, new TypeInspection(true, method));
                return true;
            }

            // Single match not found.

            method = null;
            _inspectionsCache.TryAdd(requestType, responseType, new TypeInspection(false, method));

            if (matches.Count > 1)
                throw new InvalidOperationException(
                    $"Type '{_type.Name}' has too many matching methods " +
                    $"accepting '{requestType.Name}' and returning '{responseType.Name}'.");

            return false;
        }

        private IReadOnlyList<MethodInfo> ListMatchingMethods(Type requestType, Type responseType)
        {
            return _methodInspections
                .Select(i =>
                {
                    var isMatch = _methodAnalyzer.IsMatch(i, requestType, responseType, out var match);
                    return (isMatch, match);
                })
                .Where(x => x.isMatch)
                .Select(x => x.match!)
                .ToArray();
        }

        #endregion
    }
}
