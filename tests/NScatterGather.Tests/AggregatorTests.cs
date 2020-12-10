using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            var result = await _aggregator.Send(42, timeout: TimeSpan.FromSeconds(1));

            Assert.NotNull(result);
            Assert.Equal(3, result.Completed.Count);
            Assert.Contains(typeof(SomeType), result.Completed.Select(x => x.RecipientType));
            Assert.Contains(typeof(SomeAsyncType), result.Completed.Select(x => x.RecipientType));
            Assert.Contains(typeof(SomePossiblyAsyncType), result.Completed.Select(x => x.RecipientType));

            Assert.Single(result.Faulted);
            Assert.Contains(typeof(SomeFaultingType), result.Faulted.Select(x => x.RecipientType));

            Assert.Single(result.Incomplete);
            Assert.Contains(typeof(SomeNeverEndingType), result.Incomplete.Select(x => x.RecipientType));
        }

        [Fact(Timeout = 5_000)]
        public async Task Receives_expected_response_types()
        {
            var result = await _aggregator.Send<string>(42, timeout: TimeSpan.FromSeconds(1));

            Assert.NotNull(result);
            Assert.Equal(2, result.Completed.Count);
            Assert.Contains(typeof(SomeAsyncType), result.Completed.Select(x => x.RecipientType));
            Assert.Contains(typeof(SomePossiblyAsyncType), result.Completed.Select(x => x.RecipientType));

            Assert.Single(result.Faulted);
            Assert.Contains(typeof(SomeFaultingType), result.Faulted.Select(x => x.RecipientType));

            Assert.Single(result.Incomplete);
            Assert.Contains(typeof(SomeNeverEndingType), result.Incomplete.Select(x => x.RecipientType));
        }

        [Fact]
        public async Task Responses_expose_the_recipient_name_and_type()
        {
            var collection = new RecipientsCollection();
            collection.Add((int n) => n.ToString(), "Some delegate");
            collection.Add(new SomeFaultingType(), "Some faulting type");
            collection.Add<SomeNeverEndingType>("Some never ending type");

            var localAggregator = new Aggregator(collection);
            var result = await localAggregator.Send<string>(42, timeout: TimeSpan.FromSeconds(1));

            Assert.NotNull(result);

            Assert.Equal(1, result.Completed.Count);
            Assert.Equal("Some delegate", result.Completed.First().RecipientName);
            Assert.Null(result.Completed.First().RecipientType);

            Assert.Equal(1, result.Faulted.Count);
            Assert.Equal("Some faulting type", result.Faulted.First().RecipientName);
            Assert.Equal(typeof(SomeFaultingType), result.Faulted.First().RecipientType);

            Assert.Equal(1, result.Incomplete.Count);
            Assert.Equal("Some never ending type", result.Incomplete.First().RecipientName);
            Assert.Equal(typeof(SomeNeverEndingType), result.Incomplete.First().RecipientType);
        }
    }
}
