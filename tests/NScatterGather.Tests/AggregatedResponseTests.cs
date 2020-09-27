using System;
using System.Linq;
using System.Threading.Tasks;
using NScatterGather.Invocations;
using NScatterGather.Recipients;
using Xunit;

namespace NScatterGather
{
    public class AggregatedResponseTests
    {
        private readonly Invocation<int>[] _invocations;
        private readonly Exception _ex;

        public AggregatedResponseTests()
        {
            _ex = new Exception();

            var invocation = new Invocation<int>(
                new Recipient(typeof(object)), Task.FromResult(42));

            var invocationFaulted = new Invocation<int>(
                new Recipient(typeof(bool)), Task.FromException<int>(_ex));

            var invocationIncomplete = new Invocation<int>(
                new Recipient(typeof(long)), GetInfiniteTask<int>());

            _invocations = new[] { invocation, invocationFaulted, invocationIncomplete };

            // Local functions.

            static Task<TResult> GetInfiniteTask<TResult>()
            {
                var source = new TaskCompletionSource<TResult>();
                return source.Task;
            }
        }

        [Fact]
        public void Can_be_created()
        {
            var response = new AggregatedResponse<int>(_invocations);
            Assert.Equal(_invocations.Length, response.TotalInvocationsCount);
            Assert.Single(response.Completed);
            Assert.Single(response.Faulted);
            Assert.Single(response.Incomplete);
        }

        [Fact]
        public void Invocations_are_grouped_correctly()
        {
            var response = new AggregatedResponse<int>(_invocations);

            Assert.Equal(typeof(object), response.Completed.First().recipientType);
            Assert.Equal(42, response.Completed.First().result);

            Assert.Equal(typeof(bool), response.Faulted.First().recipientType);
            Assert.Equal(_ex, response.Faulted.First().exception);

            Assert.Equal(typeof(long), response.Incomplete.First());
        }

        [Fact]
        public void Can_be_deconstructed()
        {
            var response = new AggregatedResponse<int>(_invocations);
            var (completed, faulted, incomplete) = response;
            Assert.Single(completed);
            Assert.Single(faulted);
            Assert.Single(incomplete);
        }
    }

    public class AggregatedResponseExtensionsTests
    {
        private readonly Invocation<int>[] _invocations;

        public AggregatedResponseExtensionsTests()
        {
            var invocation = new Invocation<int>(
                new Recipient(typeof(object)), Task.FromResult(42));

            var invocationFaulted = new Invocation<int>(
                new Recipient(typeof(bool)), Task.FromException<int>(new Exception()));

            var invocationIncomplete = new Invocation<int>(
                new Recipient(typeof(long)), GetInfiniteTask<int>());

            _invocations = new[] { invocation, invocationFaulted, invocationIncomplete };

            // Local functions.

            static Task<TResult> GetInfiniteTask<TResult>()
            {
                var source = new TaskCompletionSource<TResult>();
                return source.Task;
            }
        }

        [Fact]
        public void Error_if_input_is_null()
        {
            Assert.Throws<ArgumentNullException>(() =>
                (null as AggregatedResponse<int>)!.AsResultsDictionary());

            Assert.Throws<ArgumentNullException>(() =>
                (null as AggregatedResponse<int>)!.AsResultsList());
        }

        [Fact]
        public void Can_be_projected_onto_results_dictionary()
        {
            var response = new AggregatedResponse<int>(_invocations);
            var results = response.AsResultsDictionary();
            Assert.NotNull(results);
            Assert.Single(results.Keys);
            Assert.Equal(typeof(object), results.Keys.First());
            Assert.Single(results.Values);
            Assert.Equal(42, results.Values.First());
        }

        [Fact]
        public void Can_be_projected_onto_results_list()
        {
            var response = new AggregatedResponse<int>(_invocations);
            var results = response.AsResultsList();
            Assert.NotNull(results);
            Assert.Single(results, 42);
        }
    }
}
