using System;
using System.Linq;
using System.Threading.Tasks;
using NScatterGather.Recipients;
using NScatterGather.Run;
using Xunit;

namespace NScatterGather.Responses
{
    public class AggregatedResponseExtensionsTests
    {
        private readonly RecipientRunner<int>[] _runners;

        public AggregatedResponseExtensionsTests()
        {
            var runner = new RecipientRunner<int>(InstanceRecipient.Create(42));
            runner.Run(_ => Task.FromResult(42)).Wait();

            var runnerFaulted = new RecipientRunner<int>(InstanceRecipient.Create(42f));
            runnerFaulted.Run(_ => Task.FromException<int>(new Exception())).Wait();

            var runnerIncomplete = new RecipientRunner<int>(InstanceRecipient.Create(42L));
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
            var response = AggregatedResponseFactory.CreateFrom(_runners);
            var results = response.AsResultsDictionary();
            Assert.NotNull(results);
            Assert.Single(results.Keys);
            Assert.Equal(typeof(int), results.Keys.First());
            Assert.Single(results.Values);
            Assert.Equal(42, results.Values.First());
        }

        [Fact]
        public void Can_be_projected_onto_results_list()
        {
            var response = AggregatedResponseFactory.CreateFrom(_runners);
            var results = response.AsResultsList();
            Assert.NotNull(results);
            Assert.Single(results, 42);
        }
    }
}
