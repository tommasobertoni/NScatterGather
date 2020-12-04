using System;
using System.Threading.Tasks;
using NScatterGather.Recipients;

namespace NScatterGather.Samples.Samples
{
    class InvokeAsyncMethods : ISample
    {
        public async Task Run()
        {
            var collection = new RecipientsCollection();
            collection.Add<Foo>();
            collection.Add<Bar>();
            collection.Add<Baz>();

            var aggregator = new Aggregator(collection);

            // It doesn't matter if a method returns
            // synchronously or asynchronously:
            // the aggregator awaits any async method
            // before aggregating the response.
            var response1 = await aggregator.Send(42);

            var results = response1.AsResultsList(); // "42", 42L, 84L
            Console.WriteLine($"" +
                $"{results[0]} ({results[0]?.GetType().Name}), " +
                $"{results[1]} ({results[1]?.GetType().Name}), " +
                $"{results[2]} ({results[2]?.GetType().Name})");

            // The aggregator provides a "all methods are async" abstraction
            // so that when using the Send<TRequest, TResponse> method
            // all the recipients that return either TResponse, Task<TResponse>
            // or ValueTask<TResponse> get invoked.

            var response2 = await aggregator.Send<long>(42);
            var guidResults = response2.AsResultsList();
            Console.WriteLine($"{guidResults[0]} ({guidResults[0].GetType().Name})");
            Console.WriteLine($"{guidResults[0]} ({guidResults[1].GetType().Name})");

            var response3 = await aggregator.Send<string>(42);
            var stringResults = response3.AsResultsList();
            Console.WriteLine($"{stringResults[0]} ({stringResults[0].GetType().Name})");
        }

        class Foo
        {
            public long Shift(int n) => n << 1;
        }

        class Bar
        {
            public async Task<long> Longify(int n)
            {
                await Task.Yield();
                return n * 1L;
            }
        }

        class Baz
        {
            public async ValueTask<string> CreateFrom(int n)
            {
                if (n % 2 == 0)
                    await Task.Yield();

                return n.ToString();
            }
        }
    }
}
