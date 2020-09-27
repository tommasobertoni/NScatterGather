using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NScatterGather.Invocations;

namespace NScatterGather
{
    public class AggregatedResponse<TResponse>
    {
        public int TotalInvocationsCount =>
            Completed.Count +
            Faulted.Count +
            Incomplete.Count;

        public IReadOnlyList<(Type recipientType, TResponse result)> Completed { get; }

        public IReadOnlyList<(Type recipientType, Exception exception)> Faulted { get; }

        public IReadOnlyList<Type> Incomplete { get; }

        internal AggregatedResponse(
            IEnumerable<Invocation<TResponse>> invocations)
        {
            var set = new InvocationsSet<TResponse>(invocations);

            Completed = set.Completed.Select(x => (x.Recipient.Type, x.Task.Result)).ToArray();
            Faulted = set.Faulted.Select(x => (x.Recipient.Type, ExtractException(x.Task.Exception)!)).ToArray();
            Incomplete = set.Incomplete.Select(x => x.Recipient.Type).ToArray();

            // Local functions.

            static Exception? ExtractException(Exception? exception)
            {
                if (exception is null) return null;

                if (exception is AggregateException aEx)
                    return ExtractException(aEx.InnerException) ?? aEx;

                if (exception is TargetInvocationException tIEx)
                    return ExtractException(tIEx.InnerException) ?? tIEx;

                return exception;
            }
        }

        public void Deconstruct(
            out IReadOnlyList<(Type recipientType, TResponse result)> completed,
            out IReadOnlyList<(Type recipientType, Exception exception)> faulted,
            out IReadOnlyList<Type> incomplete)
        {
            completed = Completed;
            faulted = Faulted;
            incomplete = Incomplete;
        }
    }

    public static class AggregatedResponseExtensions
    {
        public static IReadOnlyDictionary<Type, TResult> AsResultsDictionary<TResult>(
            this AggregatedResponse<TResult> aggregatedResponse)
        {
            if (aggregatedResponse is null)
                throw new ArgumentNullException(nameof(aggregatedResponse));

            var dictionary = aggregatedResponse.Completed.ToDictionary(
                x => x.recipientType,
                x => x.result);

            return dictionary;
        }

        public static IReadOnlyList<TResult> AsResultsList<TResult>(
            this AggregatedResponse<TResult> aggregatedResponse)
        {
            if (aggregatedResponse is null)
                throw new ArgumentNullException(nameof(aggregatedResponse));

            var list = aggregatedResponse.Completed.Select(x => x.result).ToArray();
            return list;
        }
    }
}
