using System.Threading;
using System.Threading.Tasks;

namespace NScatterGather
{
    public class SomeAlmostNeverEndingType
    {
        private static readonly SemaphoreSlim _semaphore =
            new SemaphoreSlim(initialCount: 0);

        public async Task<string> Echo(int n, CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);
            return n.ToString();
        }
    }
}
