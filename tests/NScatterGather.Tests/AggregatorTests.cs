using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

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
            var result = await _aggregator.Send<string>(42, timeout: TimeSpan.FromSeconds(2));

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

        [Fact]
        public async Task Responses_expose_the_recipient_name_and_type()
        {
            var collection = new RecipientsCollection();
            collection.Add((int n) => n.ToString(), name: "Some delegate");
            collection.Add(new SomeFaultingType(), name: "Some faulting type");
            collection.Add<SomeNeverEndingType>(name: "Some never ending type");

            var localAggregator = new Aggregator(collection);
            var result = await localAggregator.Send<string>(42, timeout: TimeSpan.FromSeconds(2));

            Assert.NotNull(result);

            Assert.Equal(1, result.Completed.Count);
            Assert.Equal("Some delegate", result.Completed[0].RecipientName);
            Assert.Null(result.Completed[0].RecipientType);

            Assert.Equal(1, result.Faulted.Count);
            Assert.Equal("Some faulting type", result.Faulted[0].RecipientName);
            Assert.Equal(typeof(SomeFaultingType), result.Faulted[0].RecipientType);

            Assert.Equal(1, result.Incomplete.Count);
            Assert.Equal("Some never ending type", result.Incomplete[0].RecipientName);
            Assert.Equal(typeof(SomeNeverEndingType), result.Incomplete[0].RecipientType);
        }

        [Fact]
        public void Error_if_request_is_null()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _aggregator.Send((null as object)!));
            Assert.ThrowsAsync<ArgumentNullException>(() => _aggregator.Send<int>((null as object)!));
        }
    }
}
