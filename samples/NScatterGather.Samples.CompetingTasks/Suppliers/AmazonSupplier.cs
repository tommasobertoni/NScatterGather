using System.Threading.Tasks;

namespace NScatterGather.Samples.CompetingTasks
{
    class AmazonSupplier
    {
        public async Task<decimal?> PriceProduct(string productId)
        {
            await Task.Delay(200);

            return productId switch
            {
                "001" => 11.3m,
                "002" => 5.65m,
                "003" => 124.95m,
                _ => default(decimal?)
            };
        }
    }
}
