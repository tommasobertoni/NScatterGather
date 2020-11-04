using System;
using System.Linq;
using System.Threading.Tasks;
using NScatterGather.Recipients;
using NScatterGather.Run;
using Xunit;

namespace NScatterGather
{
    public class AggregatedResponseTests
    {
        private readonly RecipientRunner<int>[] _runners;
        private readonly Exception _ex;

        public AggregatedResponseTests()
        {
            _ex = new Exception();

            var runner = new RecipientRunner<int>(new Recipient(typeof(object)));
            runner.Run(_ => Task.FromResult(42));

            var runnerFaulted = new RecipientRunner<int>(new Recipient(typeof(bool)));
            runnerFaulted.Run(_ => Task.FromException<int>(new Exception()));

            var runnerIncomplete = new RecipientRunner<int>(new Recipient(typeof(long)));
            runnerIncomplete.Run(_ => GetInfiniteTask<int>());

            _runners = new[] { runner, runnerFaulted, runnerIncomplete };

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
            var response = new AggregatedResponse<int>(_runners);
            Assert.Equal(_runners.Length, response.TotalInvocationsCount);
            Assert.Single(response.Completed);
            Assert.Single(response.Faulted);
            Assert.Single(response.Incomplete);
        }

        [Fact]
        public void Invocations_are_grouped_correctly()
        {
            var response = new AggregatedResponse<int>(_runners);

            Assert.Equal(typeof(object), response.Completed.First().RecipientType);
            Assert.Equal(42, response.Completed.First().Result);

            Assert.Equal(typeof(bool), response.Faulted.First().RecipientType);
            Assert.Equal(_ex, response.Faulted.First().Exception);

            Assert.Equal(typeof(long), response.Incomplete.First().RecipientType);
        }

        [Fact]
        public void Can_be_deconstructed()
        {
            var response = new AggregatedResponse<int>(_runners);
            var (completed, faulted, incomplete) = response;
            Assert.Single(completed);
            Assert.Single(faulted);
            Assert.Single(incomplete);
        }
    }

    public class AggregatedResponseExtensionsTests
    {
        private readonly RecipientRunner<int>[] _runners;

        public AggregatedResponseExtensionsTests()
        {
            var runner = new RecipientRunner<int>(new Recipient(typeof(object)));
            runner.Run(_ => Task.FromResult(42));

            var runnerFaulted = new RecipientRunner<int>(new Recipient(typeof(bool)));
            runnerFaulted.Run(_ => Task.FromException<int>(new Exception()));

            var runnerIncomplete = new RecipientRunner<int>(new Recipient(typeof(long)));
            runnerIncomplete.Run(_ => GetInfiniteTask<int>());

            _runners = new[] { runner, runnerFaulted, runnerIncomplete };

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
            var response = new AggregatedResponse<int>(_runners);
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
            var response = new AggregatedResponse<int>(_runners);
            var results = response.AsResultsList();
            Assert.NotNull(results);
            Assert.Single(results, 42);
        }
    }
}
