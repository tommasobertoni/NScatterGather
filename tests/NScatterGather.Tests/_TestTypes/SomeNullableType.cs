using System;

namespace NScatterGather
{
    public class SomeNullableType
    {
        public DateTime? Seconds(int? seconds) => seconds is null
            ? null
            : default(DateTime).AddSeconds(seconds.Value);
    }
}
