using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NScatterGather.Internals
{
    public class CancellationGroupTests
    {
        [Fact]
        public void With_already_canceled_token()
        {
            var group = new CancellationGroup(new CancellationToken(canceled: true));
            Assert.True(group.CancellationToken.IsCancellationRequested);
        }

        [Fact]
        public void Cancel_with_one_cancellation_token()
        {
            using var cts = new CancellationTokenSource();
            var group = new CancellationGroup(cts.Token);

            Assert.False(group.CancellationToken.IsCancellationRequested);

            cts.Cancel();

            Assert.True(group.CancellationToken.IsCancellationRequested);
        }

        [Fact]
        public void Cancel_with_multiple_cancellation_tokens()
        {
            using var cts1 = new CancellationTokenSource();
            using var cts2 = new CancellationTokenSource();

            var group = new CancellationGroup(new[] { cts1.Token, cts2.Token });

            Assert.False(group.CancellationToken.IsCancellationRequested);

            cts1.Cancel();

            Assert.True(group.CancellationToken.IsCancellationRequested);

            cts2.Cancel();

            Assert.True(group.CancellationToken.IsCancellationRequested);
        }

        [Fact]
        public async Task Can_be_canceled()
        {
            var received = false;

            using var cts1 = new CancellationTokenSource();
            using var cts2 = new CancellationTokenSource();

            var group = new CancellationGroup(new[] { cts1.Token, cts2.Token });

            group.OnCanceled += () => received = true;

            group.Cancel();
            await Task.Yield();

            Assert.True(group.CancellationToken.IsCancellationRequested);
            Assert.True(received);
        }

        [Fact]
        public async Task Cancellation_emits_event()
        {
            var received = false;

            using var cts = new CancellationTokenSource();
            var group = new CancellationGroup(cts.Token);

            group.OnCanceled += () => received = true;

            cts.Cancel();
            await Task.Yield();

            Assert.True(group.CancellationToken.IsCancellationRequested);
            Assert.True(received);
        }

        [Fact(Timeout = 6000)]
        public async Task Cancellation_via_timeout()
        {
            var received = false;
            var delay = TimeSpan.FromSeconds(2);

            using var cts = new CancellationTokenSource(delay);
            var group = new CancellationGroup(cts.Token);

            group.OnCanceled += () => received = true;

            await Task.Delay(delay + delay);

            Assert.True(group.CancellationToken.IsCancellationRequested);
            Assert.True(received);
        }

        [Fact]
        public void Can_be_disposed()
        {
            using var cts = new CancellationTokenSource();
            var group = new CancellationGroup(cts.Token);

            group.Dispose();

            Assert.Throws<ObjectDisposedException>(() => group.Cancel());
        }
    }
}
