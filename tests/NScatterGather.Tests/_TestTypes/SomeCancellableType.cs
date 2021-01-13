using System.Threading;
using System.Threading.Tasks;

namespace NScatterGather
{
    public class SomeCancellableType
    {
        public async Task<string> EchoAsString(int n, CancellationToken cancellationToken)
        {
            await Task.Delay(1_000, cancellationToken);
            return n.ToString();
        }
    }
}
