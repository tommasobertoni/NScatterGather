using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static NScatterGather.CancellationHelpers;

namespace NScatterGather
{
    public class CancellationUseCasesTests
    {
        [Fact(Timeout = 5000)]
        public async Task Recipients_are_canceled_after_timeout()
        {
            var collection = new RecipientsCollection();

            collection.Add<SomeType>();
            collection.Add<SomeCancellableType>();
            collection.Add(new SomeAlmostNeverEndingType());
            collection.Add((int n) => n.ToString());
            collection.Add(async (int n, CancellationToken token) =>
            {
                using var cancellation = new CancellationTokenTaskSource<bool>(token);
                await CatchCancellation(cancellation.Task);
                return n.ToString();
            });

            var aggregator = new Aggregator(collection);
            var options = new ScatterGatherOptions { CancellationWindow = TimeSpan.FromSeconds(2) };

            {
                var response = await aggregator.Send(42, options, TimeSpan.FromSeconds(2));
                Assert.Equal(collection.RecipientsCount, response.Completed.Count);
                Assert.Empty(response.Incomplete);
            }

            {
                var response = await aggregator.Send<string>(42, options, TimeSpan.FromSeconds(2));
                Assert.Equal(collection.RecipientsCount, response.Completed.Count);
                Assert.Empty(response.Incomplete);
            }
        }

        [Fact]
        public async Task Cancellation_window_is_applied_correctly()
        {
            var collection = new RecipientsCollection();

            collection.Add<SomeAlmostNeverEndingType>();
            collection.Add<SomeLongProcessingType>();

            var aggregator = new Aggregator(collection);
            var options = new ScatterGatherOptions { CancellationWindow = TimeSpan.FromSeconds(3) };

            {
                var response = await aggregator.Send(42, options, timeout: TimeSpan.FromSeconds(1));
                Assert.Equal(1, response.Completed.Count);
                Assert.Equal(1, response.Incomplete.Count);
            }

            {
                // Gives SomeLongProcessingType the time to complete
                // even though it doesn't accept a cancellation token.
                options.AllowCancellationWindowOnAllRecipients = true;

                var response = await aggregator.Send(42, options, timeout: TimeSpan.FromSeconds(1));
                Assert.Equal(2, response.Completed.Count);
                Assert.Equal(0, response.Incomplete.Count);
            }
        }
    }
}
