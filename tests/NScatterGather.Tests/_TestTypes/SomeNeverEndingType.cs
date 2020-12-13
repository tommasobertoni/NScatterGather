using System.Threading;

namespace NScatterGather
{
    public class SomeNeverEndingType
    {
        private static readonly SemaphoreSlim _semaphore =
            new SemaphoreSlim(initialCount: 0);

        public string TryDo(int n)
        {
            _semaphore.Wait();
            return n.ToString();
        }
    }
}
