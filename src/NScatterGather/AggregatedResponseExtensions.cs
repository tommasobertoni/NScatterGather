using System;
using System.Collections.Generic;
using System.Linq;

namespace NScatterGather
{
    public static class AggregatedResponseExtensions
    {
        public static IReadOnlyDictionary<Type, TResult> AsResultsDictionary<TResult>(
            this AggregatedResponse<TResult> aggregatedResponse)
        {
            if (aggregatedResponse is null)
                throw new ArgumentNullException(nameof(aggregatedResponse));

            var dictionary = aggregatedResponse.Completed.ToDictionary(
                x => x.RecipientType,
                x => x.Result);

            return dictionary;
        }

        public static IReadOnlyList<TResult> AsResultsList<TResult>(
            this AggregatedResponse<TResult> aggregatedResponse)
        {
            if (aggregatedResponse is null)
                throw new ArgumentNullException(nameof(aggregatedResponse));

            var list = aggregatedResponse.Completed.Select(x => x.Result).ToArray();
            return list;
        }
    }
}
