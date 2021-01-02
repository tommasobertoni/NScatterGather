using System;

namespace NScatterGather
{
    public class SomeNonNullableType
    {
        public DateTime Seconds(int seconds) => default(DateTime).AddSeconds(seconds);
    }
}
