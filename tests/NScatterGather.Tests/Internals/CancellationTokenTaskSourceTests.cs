using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NScatterGather.Internals
{
    public class CancellationTokenTaskSourceTests
    {
        [Fact]
        public void Task_is_immediately_canceled()
        {
            var token = new CancellationToken(true);
            using var source = new CancellationTokenTaskSource<object>(token);
            Assert.True(source.Task.IsCanceled);
        }

        [Fact(Timeout = 5000)]
        public async Task Task_completes_after_expected_time()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            using var source = new CancellationTokenTaskSource<object>(cts.Token);

            bool canceled = false;

            try
            {
                await source.Task;
            }
            catch (TaskCanceledException)
            {
                canceled = true;
            }

            Assert.True(canceled);
        }
    }
}
