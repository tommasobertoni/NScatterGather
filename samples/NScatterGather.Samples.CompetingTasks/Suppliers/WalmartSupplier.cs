using System.Threading.Tasks;

namespace NScatterGather.Samples.CompetingTasks
{
    class WalmartSupplier
    {
        public async Task<decimal?> PriceProduct(string productId)
        {
            await Task.Delay(250);

            return productId switch
            {
                "001" => 10.99m,
                "003" => 130m,
                "004" => 271.5m,
                _ => default(decimal?)
            };
        }
    }
}
