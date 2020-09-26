using System;
using System.Collections.Concurrent;

namespace NScatterGather.Inspection
{
    internal class MethodMatchEvaluationCache
    {
        private readonly ConcurrentDictionary<int, MethodMatchEvaluation> _cache =
            new ConcurrentDictionary<int, MethodMatchEvaluation>();

        #region Request only

        public bool TryAdd<TRequest>(MethodMatchEvaluation evaluation) =>
            TryAdd(typeof(TRequest), evaluation);

        public bool TryAdd(
            Type requestType,
            MethodMatchEvaluation evaluation)
        {
            if (requestType is null) throw new ArgumentNullException(nameof(requestType));
            if (evaluation is null) throw new ArgumentNullException(nameof(evaluation));

            var hash = HashCode.Combine(requestType);
            return _cache.TryAdd(hash, evaluation);
        }

        public bool TryFindEvaluation<TRequest>(out MethodMatchEvaluation evaluation) =>
            TryFindEvaluation(typeof(TRequest), out evaluation);

        public bool TryFindEvaluation(
            Type requestType,
            out MethodMatchEvaluation evaluation)
        {
            if (requestType is null) throw new ArgumentNullException(nameof(requestType));

            var hash = HashCode.Combine(requestType);
            return _cache.TryGetValue(hash, out evaluation);
        }

        #endregion

        #region Request and response

        public bool TryAdd<TRequest, TResponse>(MethodMatchEvaluation evaluation) =>
            TryAdd(typeof(TRequest), typeof(TResponse), evaluation);

        public bool TryAdd(
            Type requestType,
            Type responseType,
            MethodMatchEvaluation evaluation)
        {
            if (requestType is null) throw new ArgumentNullException(nameof(requestType));
            if (responseType is null) throw new ArgumentNullException(nameof(responseType));
            if (evaluation is null) throw new ArgumentNullException(nameof(evaluation));

            var hash = HashCode.Combine(requestType, responseType);
            return _cache.TryAdd(hash, evaluation);
        }

        public bool TryFindEvaluation<TRequest, TResponse>(out MethodMatchEvaluation evaluation) =>
            TryFindEvaluation(typeof(TRequest), typeof(TResponse), out evaluation);

        public bool TryFindEvaluation(
            Type requestType,
            Type responseType,
            out MethodMatchEvaluation evaluation)
        {
            if (requestType is null) throw new ArgumentNullException(nameof(requestType));
            if (responseType is null) throw new ArgumentNullException(nameof(responseType));

            var hash = HashCode.Combine(requestType, responseType);
            return _cache.TryGetValue(hash, out evaluation);
        }

        #endregion
    }
}
