using System.Threading;
using System.Threading.Tasks;

namespace NScatterGather
{
    public class SomeCancellableType
    {
        public async Task<string> EchoAsString(int n, CancellationToken cancellationToken)
        {
            try { await Task.Delay(500, cancellationToken); }
            catch (TaskCanceledException) { }
            return n.ToString();
        }
    }
}
