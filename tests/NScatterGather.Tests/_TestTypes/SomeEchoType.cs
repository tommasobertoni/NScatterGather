using System.Threading.Tasks;

namespace NScatterGather
{
    public class SomeEchoType
    {
        public Task<string> Echo(int n) => Task.FromResult(n.ToString());
    }
}
