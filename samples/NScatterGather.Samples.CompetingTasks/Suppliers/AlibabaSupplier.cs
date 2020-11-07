using System.Threading.Tasks;

namespace NScatterGather.Samples.CompetingTasks
{
    class AlibabaSupplier
    {
        public async Task<decimal?> PriceProduct(string productId)
        {
            await Task.Delay(175);

            return productId switch
            {
                "003" => 99.5m,
                "004" => 284.99m,
                _ => default(decimal?)
            };
        }
    }
}
