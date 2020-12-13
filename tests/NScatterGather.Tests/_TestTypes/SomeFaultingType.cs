using System;

namespace NScatterGather
{
    public class SomeFaultingType
    {
        public string Fail(int n) => throw new Exception("A failure.");
    }
}
