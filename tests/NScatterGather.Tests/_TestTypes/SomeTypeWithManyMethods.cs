using System.Threading.Tasks;

namespace NScatterGather
{
    public class SomeTypeWithManyMethods
    {
        public void DoVoid() { }

        public void AcceptIntVoid(int n) { }

        public string EchoString(string s) => s;

        public Task DoTask() => Task.CompletedTask;

        public ValueTask DoValueTask() => new ValueTask();

        public ValueTask<int> DoAndReturnValueTask() => new ValueTask<int>(42);

        public Task<int> ReturnTask(int n) => Task.FromResult(n);

        public string Multi(int n, string s) => "Don't Panic";
    }
}
