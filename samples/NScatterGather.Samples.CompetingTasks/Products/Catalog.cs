using System.Collections.Generic;

namespace NScatterGather.Samples.CompetingTasks
{
    class Catalog
    {
        public IReadOnlyList<Product> Products { get; }

        public Catalog()
        {
            Products = new List<Product>
            {
                new Product("001", "Phone case"),
                new Product("002", "USB Type-C cable"),
                new Product("003", "Swimming pool"),
                new Product("004", "Electric scooter"),
                new Product("005", "Special item")
            };
        }
    }
}
