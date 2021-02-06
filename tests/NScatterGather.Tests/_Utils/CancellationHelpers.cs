using System;
using System.Threading.Tasks;

namespace NScatterGather
{
    internal static class CancellationHelpers
    {
        public static async Task CatchCancellation(Task cancellableTask)
        {
            try { await cancellableTask; }
            catch (OperationCanceledException) { }
        }
    }
}
