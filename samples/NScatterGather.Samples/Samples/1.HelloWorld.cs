using System;
using System.Threading.Tasks;
using NScatterGather.Recipients;

namespace NScatterGather.Samples
{
    class HelloWorld : ISample
    {
        public async Task Run()
        {
            // Register the available recipients.
            var collection = new RecipientsCollection();
            collection.Add<Foo>();
            collection.Add<Bar>();

            // Send a request and aggregate all the results.
            var aggregator = new Aggregator(collection);
            var response = await aggregator.Send(42);

            var results = response.AsResultsList(); // 1764L, 84
            Console.WriteLine($"{results[0]}, {results[1]}");
        }

        class Foo
        {
            public int Double(int n) => n * 2;
        }

        class Bar
        {
            public long Square(int n) => n * 1L * n;
        }
    }
}
