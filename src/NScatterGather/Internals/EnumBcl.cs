using System;

namespace NScatterGather
{
    internal static class EnumBcl
    {
        public static bool IsValid<TEnum>(this TEnum value) where TEnum : struct, Enum
        {
#if NETSTANDARD2_0 || NETSTANDARD2_1
            return Enum.IsDefined(typeof(TEnum), value);
#else
            return Enum.IsDefined(value);
#endif
        }
    }
}
