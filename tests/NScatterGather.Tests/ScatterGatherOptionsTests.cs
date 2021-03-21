using System;
using Xunit;

namespace NScatterGather
{
    public class ScatterGatherOptionsTests
    {
        [Fact]
        public void Cancellation_window_has_a_default()
        {
            var options = new ScatterGatherOptions();
            Assert.True(options.CancellationWindow.Ticks > 0);
        }

        [Fact]
        public void Cancellation_window_can_not_be_negative()
        {
            Assert.Throws<ArgumentException>(() => new ScatterGatherOptions
            {
                CancellationWindow = TimeSpan.FromSeconds(-1)
            });
        }

        [Fact]
        public void No_limit_by_default()
        {
            var options = new ScatterGatherOptions();
            Assert.Null(options.Limit);
        }

        [Fact]
        public void No_limit_can_be_set()
        {
            int limit = 5;
            var options = new ScatterGatherOptions { Limit = limit };
            Assert.Equal(limit, options.Limit);
        }
    }
}
