using System;
using System.Threading.Tasks;
using NScatterGather.Recipients;

namespace NScatterGather.Samples.Samples
{
    class DelegateRecipients : ISample
    {
        public async Task Run()
        {
            var collection = new RecipientsCollection();

            // Register typeless recipients using delegates.
            collection.Add((int n) => n.ToString());
            collection.Add((int n) => n * 2);
            collection.Add((DateTime d) => d.Ticks);

            var aggregator = new Aggregator(collection);

            var responseOfInt = await aggregator.Send(42);
            var resultsOfInt = responseOfInt.AsResultsList(); // 84, "42"
            Console.WriteLine($"{resultsOfInt[0]}, {resultsOfInt[1]}");

            var onlyStrings = await aggregator.Send<int, string>(42);
            var onlyStringResults = onlyStrings.AsResultsList(); // "42"
            Console.WriteLine($"{onlyStringResults[0]}");

            var responseOfDateTime = await aggregator.Send(DateTime.UtcNow);
            var resultsOfDateTime = responseOfDateTime.AsResultsList(); // 'now.Ticks'L
            Console.WriteLine($"{resultsOfDateTime[0]}");
        }
    }
}
