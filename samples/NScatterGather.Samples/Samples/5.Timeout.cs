using System;
using System.Threading;
using System.Threading.Tasks;
using NScatterGather.Recipients;

namespace NScatterGather.Samples.Samples
{
    class Timeout : ISample
    {
        public async Task Run()
        {
            var collection = new RecipientsCollection();
            collection.Add<Foo>();
            collection.Add<Bar>();
            collection.Add<Baz>();

            var aggregator = new Aggregator(collection);

            // The consumer can limit the duration of the aggregation
            // by providing a CancellationToken to the aggregator:
            // this will ensure that the response will be ready in
            // the given amount of time and will "discard" incomplete
            // (and also never-ending) invocations.
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
            var response = await aggregator.Send(42, cts.Token);

            // The recipients that didn't complete in time will be
            // listed in the Incomplete property of the result:
            Console.WriteLine($"Completed {response.Completed.Count}");
            Console.WriteLine(
                $"Incomplete {response.Incomplete.Count}: " +
                $"{response.Incomplete[0].RecipientType?.Name}");
        }

        class Foo
        {
            public int Fast(int n) => n;
        }

        class Bar
        {
            public async Task<int> Long(int n)
            {
                await Task.Delay(100);
                return n;
            }
        }

        class Baz
        {
            public Task<int> Block(int n)
            {
                var tcs = new TaskCompletionSource<int>();
                return tcs.Task; // It will never complete.
            }
        }
    }
}
