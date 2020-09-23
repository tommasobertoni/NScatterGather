using System;
using System.Collections.Concurrent;

namespace NScatterGather.Inspection
{
    internal class TypeInspectionCache
    {
        private readonly ConcurrentDictionary<int, TypeInspection> _cache =
            new ConcurrentDictionary<int, TypeInspection>();

        #region Request only

        public bool TryAdd<TRequest>(TypeInspection inspection) =>
            TryAdd(typeof(TRequest), inspection);

        public bool TryAdd(
            Type requestType,
            TypeInspection inspection)
        {
            if (requestType is null) throw new ArgumentNullException(nameof(requestType));

            var hash = HashCode.Combine(requestType);
            return _cache.TryAdd(hash, inspection);
        }

        public bool TryFindInspectionResult<TRequest>(out TypeInspection inspection) =>
            TryFindInspectionResult(typeof(TRequest), out inspection);

        public bool TryFindInspectionResult(
            Type requestType,
            out TypeInspection inspection)
        {
            if (requestType is null) throw new ArgumentNullException(nameof(requestType));

            var hash = HashCode.Combine(requestType);
            return _cache.TryGetValue(hash, out inspection);
        }

        #endregion

        #region Request and response

        public bool TryAdd<TRequest, TResponse>(TypeInspection inspection) =>
            TryAdd(typeof(TRequest), typeof(TResponse), inspection);

        public bool TryAdd(
            Type requestType,
            Type responseType,
            TypeInspection inspection)
        {
            if (requestType is null) throw new ArgumentNullException(nameof(requestType));
            if (responseType is null) throw new ArgumentNullException(nameof(responseType));

            var hash = HashCode.Combine(requestType, responseType);
            return _cache.TryAdd(hash, inspection);
        }

        public bool TryFindInspectionResult<TRequest, TResponse>(out TypeInspection inspection) =>
            TryFindInspectionResult(typeof(TRequest), typeof(TResponse), out inspection);

        public bool TryFindInspectionResult(
            Type requestType,
            Type responseType,
            out TypeInspection inspection)
        {
            if (requestType is null) throw new ArgumentNullException(nameof(requestType));
            if (responseType is null) throw new ArgumentNullException(nameof(responseType));

            var hash = HashCode.Combine(requestType, responseType);
            return _cache.TryGetValue(hash, out inspection);
        }

        #endregion
    }
}
