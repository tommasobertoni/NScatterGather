using System;
using System.Threading.Tasks;

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

            // The consumer can limit the duration of the aggregation by
            // providing a timeout (or CancellationToken) to the aggregator:
            // this will ensure that the response will be ready in
            // the given amount of time and will "discard" incomplete
            // (and also never-ending) invocations.
            var response = await aggregator.Send(42, TimeSpan.FromMilliseconds(50));

            // The recipients that didn't complete in time will be
            // listed in the Incomplete property of the result:
            Console.WriteLine($"Completed {response.Completed.Count}");
            Console.WriteLine(
                $"Incomplete {response.Incomplete.Count}: " +
                $"{response.Incomplete[0].RecipientType?.Name}, " +
                $"{response.Incomplete[1].RecipientType?.Name}");
        }

        class Foo
        {
            public int Fast(int n) => n;
        }

        class Bar
        {
            public async Task<int> Long(int n)
            {
                await Task.Delay(1000);
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
