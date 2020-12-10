using System;
using System.Threading.Tasks;
using NScatterGather.Recipients;
using NScatterGather.Run;
using Xunit;

namespace NScatterGather.Responses
{
    public class AggregatedResponseTests
    {
        private readonly RecipientRunner<int>[] _runners;
        private readonly Exception _ex;

        public AggregatedResponseTests()
        {
            _ex = new Exception("Test ex.");

            var runner = new RecipientRunner<int>(new InstanceRecipient(typeof(object)));
            runner.Run(_ => Task.FromResult(42)).Wait();

            var runnerFaulted = new RecipientRunner<int>(new InstanceRecipient(typeof(bool)));
            runnerFaulted.Run(_ => Task.FromException<int>(_ex)).Wait();

            var runnerIncomplete = new RecipientRunner<int>(new InstanceRecipient(typeof(long)));
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
            var response = AggregatedResponseFactory.CreateFrom(_runners);
            Assert.Equal(_runners.Length, response.TotalInvocationsCount);
            Assert.Single(response.Completed);
            Assert.Single(response.Faulted);
            Assert.Single(response.Incomplete);
        }

        [Fact]
        public void Invocations_are_grouped_correctly()
        {
            var response = AggregatedResponseFactory.CreateFrom(_runners);

            Assert.Equal(typeof(object), response.Completed[0].RecipientType);
            Assert.Equal(42, response.Completed[0].Result);

            Assert.Equal(typeof(bool), response.Faulted[0].RecipientType);
            Assert.Equal(_ex, response.Faulted[0].Exception);

            Assert.Equal(typeof(long), response.Incomplete[0].RecipientType);
        }

        [Fact]
        public void Can_be_deconstructed()
        {
            var response = AggregatedResponseFactory.CreateFrom(_runners);
            var (completed, faulted, incomplete) = response;
            Assert.Single(completed);
            Assert.Single(faulted);
            Assert.Single(incomplete);
        }
    }
}
