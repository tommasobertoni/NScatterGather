using System.Threading.Tasks;

namespace NScatterGather
{
    public class SomeAsyncType
    {
        public Task<string> EchoAsString(int n) => Task.FromResult(n.ToString());
    }
}
