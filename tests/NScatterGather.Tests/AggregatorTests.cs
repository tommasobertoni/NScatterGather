using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NScatterGather.Recipients;
using Xunit;

namespace NScatterGather
{
    public class AggregatorTests
    {
        class SomeType
        {
            public int Do(int n) => n * 2;
        }

        class SomeAsyncType
        {
            public Task<string> Do(int n) => Task.FromResult(n.ToString());
        }

        class SomePossiblyAsyncType
        {
            public ValueTask<string> Do(int n) => new ValueTask<string>(n.ToString());
        }

        class SomeCollidingType
        {
            public string Do(int n) => n.ToString();

            public string DoDifferently(int n) => $"{n}";
        }

        class SomeFaultingType
        {
            public string Fail(int n) => throw new Exception("A failure.");
        }

        class SomeNeverEndingType
        {
            private static readonly SemaphoreSlim _semaphore =
                new SemaphoreSlim(initialCount: 0);

            public string TryDo(int n)
            {
                _semaphore.Wait();
                return n.ToString();
            }
        }

        private readonly Aggregator _aggregator;

        public AggregatorTests()
        {
            var collection = new RecipientsCollection();
            collection.Add<SomeType>();
            collection.Add<SomeAsyncType>();
            collection.Add<SomePossiblyAsyncType>();
            collection.Add<SomeCollidingType>();
            collection.Add<SomeFaultingType>();
            collection.Add<SomeNeverEndingType>();

            _aggregator = new Aggregator(collection);
        }

        [Fact(Timeout = 5_000)]
        public async Task Sends_request_and_aggregates_responses()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(0.5));
            var result = await _aggregator.Send(42, cts.Token);

            Assert.NotNull(result);
            Assert.Equal(3, result.Completed.Count);
            Assert.Contains(typeof(SomeType), result.Completed.Select(x => x.recipientType));
            Assert.Contains(typeof(SomeAsyncType), result.Completed.Select(x => x.recipientType));
            Assert.Contains(typeof(SomePossiblyAsyncType), result.Completed.Select(x => x.recipientType));

            Assert.Single(result.Faulted);
            Assert.Contains(typeof(SomeFaultingType), result.Faulted.Select(x => x.recipientType));

            Assert.Single(result.Incomplete);
            Assert.Contains(typeof(SomeNeverEndingType), result.Incomplete);
        }

        [Fact(Timeout = 5_000)]
        public async Task Receives_expected_response_types()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(0.5));
            var result = await _aggregator.Send<int, string>(42, cts.Token);

            Assert.NotNull(result);
            Assert.Equal(2, result.Completed.Count);
            Assert.Contains(typeof(SomeAsyncType), result.Completed.Select(x => x.recipientType));
            Assert.Contains(typeof(SomePossiblyAsyncType), result.Completed.Select(x => x.recipientType));

            Assert.Single(result.Faulted);
            Assert.Contains(typeof(SomeFaultingType), result.Faulted.Select(x => x.recipientType));

            Assert.Single(result.Incomplete);
            Assert.Contains(typeof(SomeNeverEndingType), result.Incomplete);
        }
    }
}
