using System;
using System.Threading.Tasks;

namespace NScatterGather.Samples.Samples
{
    class FilterOnResponse : ISample
    {
        public async Task Run()
        {
            // Recipients are registered as always.
            var collection = new RecipientsCollection();
            collection.Add<Foo>();
            collection.Add<Bar>();

            var aggregator = new Aggregator(collection);

            // The Send<TRequest> method checks only the input
            // parameter of the methods:
            AggregatedResponse<object?> all = await aggregator.Send(42);

            var allResults = all.AsResultsList(); // 42L, "42"
            Console.WriteLine($"" +
                $"{allResults[0]} ({allResults[0]?.GetType().Name}), " +
                $"{allResults[1]} ({allResults[1]?.GetType().Name})");

            // Instead the Send<TRequest, TResponse> method checks
            // also the return type of the methods, allowing to filter
            // on them and getting typed results:
            AggregatedResponse<string> onlyStrings = await aggregator.Send<string>(42);

            var onlyStringsResults = onlyStrings.AsResultsList(); // "42"
            Console.WriteLine($"{onlyStringsResults[0]} ({allResults[0]?.GetType().Name})");
        }

        class Foo
        {
            public string Stringify(int n) => n.ToString();
        }

        class Bar
        {
            public long Longify(int n) => n * 1L;
        }
    }
}
