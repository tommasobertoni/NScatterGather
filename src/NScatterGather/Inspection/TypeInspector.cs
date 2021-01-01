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

        public Type Type => _type;

        private readonly MethodMatchEvaluationCache _evaluationsCache = new MethodMatchEvaluationCache();

        private readonly IReadOnlyList<MethodInspection> _methodInspections;
        private readonly Type _type;

        public TypeInspector(Type type)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _methodInspections = InspectMethods(type);

            // Local functions.

            static IReadOnlyList<MethodInspection> InspectMethods(Type type)
            {
                var methods = type.GetMethods(DefaultFlags);
                return methods
                    .Where(method => method.DeclaringType == type)
                    .Select(method => new MethodInspection(type, method))
                    .Where(inspection => inspection.ReturnsAResponse)
                    .ToArray();
            }
        }

        #region Request only
        public bool HasMethodsAccepting(Type requestType, CollisionStrategy collisionStrategy) =>
            TryGetMethodsAccepting(requestType, collisionStrategy, out _);

        public bool TryGetMethodsAccepting(
            Type requestType,
            CollisionStrategy collisionStrategy,
            out IReadOnlyList<MethodInfo> methods)
        {
            if (requestType is null)
                throw new ArgumentNullException(nameof(requestType));

            if (!collisionStrategy.IsValid())
                throw new ArgumentException($"Invalid {nameof(collisionStrategy)} value: {collisionStrategy}");

            var evaluation = FindOrEvaluate(requestType);

            return IsCompliantWithCollisionStrategyOrThrow(
                requestType,
                responseType: null,
                evaluation,
                collisionStrategy,
                out methods);
        }

        private MethodMatchEvaluation FindOrEvaluate(Type requestType)
        {
            // Check in the cache if the analysis was already done.
            if (_evaluationsCache.TryFindEvaluation(requestType, out var cached))
                return cached;

            var matches = ListMatchingMethods(requestType);

            var evaluation = new MethodMatchEvaluation(requestType, responseType: null, matches);
            _evaluationsCache.TryAdd(evaluation);

            return evaluation;
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

        public bool HasMethodsReturning(Type requestType, Type responseType, CollisionStrategy collisionStrategy) =>
            TryGetMethodsReturning(requestType, responseType, collisionStrategy, out _);

        public bool TryGetMethodsReturning(
            Type requestType,
            Type responseType,
            CollisionStrategy collisionStrategy,
            out IReadOnlyList<MethodInfo> methods)
        {
            if (requestType is null)
                throw new ArgumentNullException(nameof(requestType));

            if (responseType is null)
                throw new ArgumentNullException(nameof(responseType));

            if (!collisionStrategy.IsValid())
                throw new ArgumentException($"Invalid {nameof(collisionStrategy)} value: {collisionStrategy}");

            var evaluation = FindOrEvaluate(requestType, responseType);

            return IsCompliantWithCollisionStrategyOrThrow(
                requestType,
                responseType,
                evaluation,
                collisionStrategy,
                out methods);
        }

        private MethodMatchEvaluation FindOrEvaluate(Type requestType, Type responseType)
        {
            // Check in the cache if the analysis was already done.
            if (_evaluationsCache.TryFindEvaluation(requestType, responseType, out var cached))
                return cached;

            var matches = ListMatchingMethods(requestType, responseType);

            var evaluation = new MethodMatchEvaluation(requestType, responseType, matches);
            _evaluationsCache.TryAdd(evaluation);

            return evaluation;
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

        private bool IsCompliantWithCollisionStrategyOrThrow(
            Type requestType,
            Type? responseType,
            MethodMatchEvaluation evaluation,
            CollisionStrategy collisionStrategy,
            out IReadOnlyList<MethodInfo> methods)
        {
            var (_, _, matches) = evaluation;

            // Check if evaluation has methods and is compliant with the CollisionStrategy

            if (matches.Count == 0)
            {
                methods = Array.Empty<MethodInfo>();
                return false;
            }
            else if (matches.Count == 1)
            {
                methods = matches;
                return true;
            }

            // More than one match found

            if (collisionStrategy == CollisionStrategy.UseAllMethodsMatching)
            {
                methods = matches;
                return true;
            }

            throw new CollisionException(_type, requestType, responseType);
        }
    }
}
