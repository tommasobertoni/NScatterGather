using System;
using System.Linq;
using System.Threading.Tasks;
using NScatterGather.Recipients;

namespace NScatterGather.Samples.Samples
{
    class HandleErrors : ISample
    {
        public async Task Run()
        {
            var collection = new RecipientsCollection();
            collection.Add<Foo>();
            collection.Add<Bar>();

            var aggregator = new Aggregator(collection);

            var response = await aggregator.Send("Don't Panic");

            // The aggregated response separates the invocations
            // that completed successfully with a response and
            // the ones that failed with an exception.
            Console.WriteLine($"Completed {response.Completed.Count}");
            Console.WriteLine(
                $"Faulted {response.Faulted.Count}: " +
                $"{response.Faulted[0].RecipientType.Name} => " +
                $"{response.Faulted[0].Exception.Message}");
        }

        class Foo
        {
            public string Get(string s) =>
                new string(s.Reverse().ToArray());
        }

        class Bar
        {
            public string Todo(string s) =>
                throw new NotImplementedException("TODO");
        }
    }
}
