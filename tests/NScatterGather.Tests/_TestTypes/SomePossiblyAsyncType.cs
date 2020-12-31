using System.Threading.Tasks;

namespace NScatterGather
{
    public class SomePossiblyAsyncType
    {
        public ValueTask<string> Do(int n) => new ValueTask<string>(n.ToString());
    }
}
