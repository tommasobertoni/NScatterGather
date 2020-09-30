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

            var aggregator = new Aggregator(collection);

            // It doesn't matter if a method returns
            // synchronously or asynchronously:
            // the aggregator awaits any async method
            // before aggregating the response.
            var response1 = await aggregator.Send(42);

            var results = response1.AsResultsList(); // 0000002a-0001-0002-0304-050607080900, 84L
            Console.WriteLine($"" +
                $"{results[0]} ({results[0].GetType().Name}), " +
                $"{results[1]} ({results[1].GetType().Name})");

            // The aggregator provides a "all methods are async" abstraction
            // so that when using the Send<TRequest, TResponse> method
            // all the recipients that return either TResponse, Task<TResponse>
            // or ValueTask<TResponse> get invoked.
            var response2 = await aggregator.Send<int, Guid>(42);

            var guidResults = response2.AsResultsList();
            Console.WriteLine($"" +
                $"{guidResults[0]} ({guidResults[0].GetType().Name})");
        }

        class Foo
        {
            public long Shift(int n) => n << 1;
        }

        class Bar
        {
            public async Task<Guid> CreateFrom(int n)
            {
                await Task.Yield();
                var guid = new Guid(n, 1, 2, new byte[] { 3, 4, 5, 6, 7, 8, 9, 0 });
                return guid;
            }
        }
    }
}
