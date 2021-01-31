using System.Threading.Tasks;

namespace NScatterGather
{
    public class SomeLongProcessingType
    {
        public async Task<string> Process(int n)
        {
            await Task.Delay(2_000);
            return n.ToString();
        }
    }
}
