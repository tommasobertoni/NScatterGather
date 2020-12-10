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

        private readonly MethodMatchEvaluationCache _evaluationsCache = new MethodMatchEvaluationCache();

        private readonly IReadOnlyList<MethodInspection> _methodInspections;
        private readonly Type _type;

        public TypeInspector(Type type)
        {
            _type = type;
            _methodInspections = InspectMethods(type);

            // Local functions.

            static IReadOnlyList<MethodInspection> InspectMethods(Type type)
            {
                var methods = type.GetMethods(DefaultFlags);
                return methods
                    .Select(method => new MethodInspection(type, method))
                    .Where(inspection => inspection.ReturnsAResponse)
                    .ToArray();
            }
        }

        #region Request only
        public bool HasMethodAccepting(Type requestType) =>
            TryGetMethodAccepting(requestType, out _);

        public bool TryGetMethodAccepting(
            Type requestType,
            [NotNullWhen(true)] out MethodInfo? method)
        {
            if (requestType is null) throw new ArgumentNullException(nameof(requestType));

            // Check in the cache if the analysis was already done.
            if (_evaluationsCache.TryFindEvaluation(requestType, out var cached))
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
                _evaluationsCache.TryAdd(requestType, new MethodMatchEvaluation(true, method));
                return true;
            }

            // Single match not found.

            method = null;
            _evaluationsCache.TryAdd(requestType, new MethodMatchEvaluation(false, method));

            if (matches.Count > 1)
                throw new ConflictException(_type, requestType);

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

        public bool HasMethodReturning(Type requestType, Type responseType) =>
            TryGetMethodReturning(requestType, responseType, out _);

        public bool TryGetMethodReturning(
            Type requestType,
            Type responseType,
            [NotNullWhen(true)] out MethodInfo? method)
        {
            if (requestType is null) throw new ArgumentNullException(nameof(requestType));
            if (responseType is null) throw new ArgumentNullException(nameof(responseType));

            // Check in the cache if the analysis was already done.
            if (_evaluationsCache.TryFindEvaluation(requestType, responseType, out var cached))
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
                _evaluationsCache.TryAdd(requestType, responseType, new MethodMatchEvaluation(true, method));
                return true;
            }

            // Single match not found.

            method = null;
            _evaluationsCache.TryAdd(requestType, responseType, new MethodMatchEvaluation(false, method));

            if (matches.Count > 1)
                throw new ConflictException(_type, requestType, responseType);

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
