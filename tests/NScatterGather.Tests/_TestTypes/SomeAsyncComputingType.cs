using System.Threading.Tasks;

namespace NScatterGather
{
    public class SomeAsyncComputingType
    {
        public Task Do(int n) => Task.CompletedTask;
    }
}
