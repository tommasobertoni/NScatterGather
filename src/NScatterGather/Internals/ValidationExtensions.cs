using System;

namespace NScatterGather
{
    internal static class ValidationExtensions
    {
        public static bool IsNegative(this TimeSpan timeSpan) =>
            timeSpan.Ticks < 0;
    }
}
