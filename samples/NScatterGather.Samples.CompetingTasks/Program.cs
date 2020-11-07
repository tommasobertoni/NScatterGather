using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NScatterGather.Invocations;
using NScatterGather.Recipients;
using static System.Console;

namespace NScatterGather.Samples.CompetingTasks
{
    class Program
    {
        static async Task Main()
        {
            var recipients = CollectRecipients();
            var aggregator = new Aggregator(recipients);

            var catalog = new Catalog();

            WriteLine();

            foreach (var product in catalog.Products)
            {
                var response = await aggregator.Send<string, decimal?>(product.Id);

                PrettyPrint(product, response);

                WriteLine();
                WriteLine("---------------------------------------");
                WriteLine();
            }
        }

        private static void PrettyPrint(
            Product product,
            AggregatedResponse<decimal?> response)
        {
            WriteLine($"Product: '{product.Name}'");
            WriteLine();

            if (IsProductOutOfStock(response.Completed))
            {
                WriteLine("  Out of stock.");
                return;
            }

            var bestPrice = response.Completed.Where(x => x.Result.HasValue).Min(x => x.Result!.Value);

            foreach (var invocation in response.Completed.Where(x => x.Result.HasValue).OrderBy(x => x.Duration))
            {
                var supplierName = GetSupplierName(invocation.RecipientType);
                var supplierPrice = invocation.Result!.Value;

                var output = $"  ${invocation.Result!.Value} (supplier: {supplierName})";

                if (supplierPrice == bestPrice)
                    output += $" -- best";

                WriteLine(output);
            }
        }

        private static bool IsProductOutOfStock(IReadOnlyList<CompletedInvocation<decimal?>> invocations)
        {
            return invocations.All(x => x.Result == null);
        }

        private static object GetSupplierName(Type recipientType)
        {
            return recipientType.Name.Replace("Supplier", "");
        }

        private static RecipientsCollection CollectRecipients()
        {
            var collection = new RecipientsCollection();
            collection.Add<AlibabaSupplier>();
            collection.Add<AmazonSupplier>();
            collection.Add<WalmartSupplier>();
            return collection;
        }
    }
}
