using System;
using Xunit;

namespace NScatterGather.Inspection
{
    public class ValidationExtensionsTests
    {
        [Fact]
        public void Timespan_is_negative()
        {
            Assert.False(TimeSpan.FromSeconds(1).IsNegative());
            Assert.False(TimeSpan.FromSeconds(0).IsNegative());
            Assert.True(TimeSpan.FromSeconds(-1).IsNegative());
        }
    }
}
