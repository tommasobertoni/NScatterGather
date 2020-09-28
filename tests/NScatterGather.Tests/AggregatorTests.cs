using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
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

        class SomeCollidingType
        {
            public string Do(int n) => n.ToString();

            public string DoDifferently(int n) => $"{n}";
        }

        class SomeFaultingType
        {
            public string Fail(int n) => throw new Exception("A failure.");
        }

        private readonly Aggregator _aggregator;

        public AggregatorTests()
        {
            var mock = new Mock<ILogger<RecipientsCollection>>();

            var collection = new RecipientsCollection(mock.Object);
            collection.Add<SomeType>();
            collection.Add<SomeAsyncType>();
            collection.Add<SomeCollidingType>();
            collection.Add<SomeFaultingType>();

            _aggregator = new Aggregator(collection);
        }

        [Fact(Timeout = 2_000)]
        public async Task Sends_request_and_aggregates_responses()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            var result = await _aggregator.Send(42, cts.Token);

            Assert.NotNull(result);
            Assert.Equal(2, result.Completed.Count);
            Assert.Contains(typeof(SomeType), result.Completed.Select(x => x.recipientType));
            Assert.Contains(typeof(SomeAsyncType), result.Completed.Select(x => x.recipientType));

            Assert.Single(result.Faulted);
            Assert.Contains(typeof(SomeFaultingType), result.Faulted.Select(x => x.recipientType));

            Assert.Empty(result.Incomplete);
        }

        [Fact(Timeout = 2_000)]
        public async Task Receives_expected_response_types()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            var result = await _aggregator.Send<int, string>(42, cts.Token);

            Assert.NotNull(result);
            Assert.Equal(1, result.Completed.Count);
            Assert.Contains(typeof(SomeAsyncType), result.Completed.Select(x => x.recipientType));

            Assert.Single(result.Faulted);
            Assert.Contains(typeof(SomeFaultingType), result.Faulted.Select(x => x.recipientType));

            Assert.Empty(result.Incomplete);
        }
    }
}
