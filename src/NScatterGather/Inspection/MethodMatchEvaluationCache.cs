using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace NScatterGather.Inspection
{
    internal class MethodMatchEvaluationCache
    {
        private readonly ConcurrentDictionary<int, MethodMatchEvaluation> _cache =
            new ConcurrentDictionary<int, MethodMatchEvaluation>();

        public bool TryAdd(MethodMatchEvaluation evaluation)
        {
            if (evaluation is null) throw new ArgumentNullException(nameof(evaluation));
            if (evaluation.RequestType is null) throw new ArgumentNullException(nameof(evaluation.RequestType));
            if (evaluation.Methods is null) throw new ArgumentNullException(nameof(evaluation.Methods));

            var hash = evaluation.ResponseType is null
                ? HashCode.Combine(evaluation.RequestType)
                : HashCode.Combine(evaluation.RequestType, evaluation.ResponseType);

            return _cache.TryAdd(hash, evaluation);
        }

        public bool TryFindEvaluation(
            Type requestType,
            [NotNullWhen(true)] out MethodMatchEvaluation? evaluation)
        {
            if (requestType is null) throw new ArgumentNullException(nameof(requestType));

            var hash = HashCode.Combine(requestType);
            return _cache.TryGetValue(hash, out evaluation);
        }

        public bool TryFindEvaluation(
            Type requestType,
            Type responseType,
            [NotNullWhen(true)] out MethodMatchEvaluation? evaluation)
        {
            if (requestType is null) throw new ArgumentNullException(nameof(requestType));
            if (responseType is null) throw new ArgumentNullException(nameof(responseType));

            var hash = HashCode.Combine(requestType, responseType);
            return _cache.TryGetValue(hash, out evaluation);
        }
    }
}
