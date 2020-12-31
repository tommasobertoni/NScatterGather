using System;
using System.Reflection;
using System.Threading.Tasks;

namespace NScatterGather
{
    public class SomeComplexFaultingType
    {
        public Task<int> Fail(int n)
        {
            Task.WhenAll(Fail(), Fail(), Fail()).Wait();
            return Task.FromResult(n);

            // Local functions.

            static async Task Fail()
            {
                await Task.Delay(10);
                throw new Exception("A failure.");
            }
        }

        public Task<int> FailInvocation(long n)
        {
            throw new TargetInvocationException(new Exception("An invocation failure."));
        }
    }
}
