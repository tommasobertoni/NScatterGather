using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static NScatterGather.CancellationHelpers;

namespace NScatterGather
{
    public class AggregatorTests
    {
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
            var result = await _aggregator.Send(42, timeout: TimeSpan.FromSeconds(2));

            Assert.NotNull(result);
            Assert.Equal(3, result.Completed.Count);
            Assert.Contains(typeof(SomeType), result.Completed.Select(x => x.Recipient.Type));
            Assert.Contains(typeof(SomeAsyncType), result.Completed.Select(x => x.Recipient.Type));
            Assert.Contains(typeof(SomePossiblyAsyncType), result.Completed.Select(x => x.Recipient.Type));

            Assert.Single(result.Faulted);
            Assert.Contains(typeof(SomeFaultingType), result.Faulted.Select(x => x.Recipient.Type));

            Assert.Single(result.Incomplete);
            Assert.Contains(typeof(SomeNeverEndingType), result.Incomplete.Select(x => x.Recipient.Type));
        }

        [Fact(Timeout = 5_000)]
        public async Task Receives_expected_response_types()
        {
            var result = await _aggregator.Send<string>(42, timeout: TimeSpan.FromSeconds(2));

            Assert.NotNull(result);
            Assert.Equal(3, result.Completed.Count);
            Assert.Contains(typeof(SomeType), result.Completed.Select(x => x.Recipient.Type));
            Assert.Contains(typeof(SomeAsyncType), result.Completed.Select(x => x.Recipient.Type));
            Assert.Contains(typeof(SomePossiblyAsyncType), result.Completed.Select(x => x.Recipient.Type));

            Assert.Single(result.Faulted);
            Assert.Contains(typeof(SomeFaultingType), result.Faulted.Select(x => x.Recipient.Type));

            Assert.Single(result.Incomplete);
            Assert.Contains(typeof(SomeNeverEndingType), result.Incomplete.Select(x => x.Recipient.Type));
        }

        [Fact]
        public void Error_if_request_is_null()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _aggregator.Send((null as object)!));
            Assert.ThrowsAsync<ArgumentNullException>(() => _aggregator.Send<int>((null as object)!));
        }

        [Fact]
        public void Cancellation_window_can_not_be_negative()
        {
            Assert.Throws<ArgumentException>(() => _aggregator.CancellationWindow = TimeSpan.FromSeconds(-1));
        }
    }
}
