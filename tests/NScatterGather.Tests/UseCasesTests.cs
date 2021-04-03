using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NScatterGather
{
    public class UseCasesTests
    {
        [Fact(Timeout = 5000)]
        public async Task Recipients_comply_with_lifetime()
        {
            var transients = 0;
            var scoped = 0;
            var singletons = 0;

            var collection = new RecipientsCollection();

            collection.Add(() => { transients++; return new SomeType(); }, name: null, lifetime: Lifetime.Transient);
            collection.Add(() => { scoped++; return new SomeOtherType(); }, name: null, lifetime: Lifetime.Scoped);
            collection.Add(() => { singletons++; return new SomeAsyncType(); }, name: null, lifetime: Lifetime.Singleton);

            var aggregator = new Aggregator(collection);
            var anotherAggregator = new Aggregator(collection);

            await aggregator.Send(42);

            Assert.Equal(1, transients);
            Assert.Equal(1, scoped);
            Assert.Equal(1, singletons);

            await anotherAggregator.Send(42);

            Assert.Equal(2, transients);
            Assert.Equal(2, scoped);
            Assert.Equal(1, singletons);

            await Task.WhenAll(aggregator.Send(42), anotherAggregator.Send(42));

            Assert.Equal(4, transients);
            Assert.Equal(2, scoped);
            Assert.Equal(1, singletons);
        }

        [Fact(Timeout = 5000)]
        public async Task Recipients_can_return_null()
        {
            var collection = new RecipientsCollection();
            collection.Add<SomeTypeReturningNull>();

            var aggregator = new Aggregator(collection);

            var response = await aggregator.Send(42);

            Assert.NotNull(response);
            Assert.Single(response.Completed);
            Assert.Empty(response.Faulted);

            var completed = response.Completed[0];
            Assert.Equal(typeof(SomeTypeReturningNull), completed.Recipient.Type);
            Assert.Null(completed.Result);
        }

        [Fact(Timeout = 5000)]
        public async Task Recipients_can_return_nullable()
        {
            var collection = new RecipientsCollection();
            collection.Add<SomeTypeReturningNullable>();

            var aggregator = new Aggregator(collection);

            var response = await aggregator.Send(42);

            Assert.NotNull(response);
            Assert.Single(response.Completed);
            Assert.Empty(response.Faulted);

            var completed = response.Completed[0];
            Assert.Equal(typeof(SomeTypeReturningNullable), completed.Recipient.Type);
            Assert.Null(completed.Result);
        }

        [Fact(Timeout = 5000)]
        public async Task Colliding_recipients_are_ignored_by_design()
        {
            var collection = new RecipientsCollection();
            collection.Add<SomeCollidingType>(CollisionStrategy.IgnoreRecipient);

            var collisionDetected = false;
            collection.OnCollision += _ => collisionDetected = true;

            var aggregator = new Aggregator(collection);

            var (completed, faulted, incomplete) = await aggregator.Send(42);

            Assert.Empty(completed);
            Assert.Empty(faulted);
            Assert.Empty(incomplete);

            Assert.True(collisionDetected);
        }

        [Fact(Timeout = 5000)]
        public async Task Colliding_recipients_use_all_methods_by_design()
        {
            var collection = new RecipientsCollection();
            collection.Add<SomeCollidingType>(CollisionStrategy.UseAllMethodsMatching);

            var collisionDetected = false;
            collection.OnCollision += _ => collisionDetected = true;

            var aggregator = new Aggregator(collection);

            var (completed, faulted, incomplete) = await aggregator.Send(42);

            Assert.Equal(2, completed.Count);
            Assert.Empty(faulted);
            Assert.Empty(incomplete);

            Assert.False(collisionDetected);

            var stringsOnly = await aggregator.Send<string>(42);

            Assert.Equal(2, stringsOnly.Completed.Count);
            Assert.Empty(stringsOnly.Faulted);
            Assert.Empty(stringsOnly.Incomplete);

            Assert.False(collisionDetected);
        }

        [Fact(Timeout = 5000)]
        public async Task Responses_expose_the_recipient_name_and_type()
        {
            var collection = new RecipientsCollection();
            collection.Add((int n) => n.ToString(), name: "Some delegate");
            collection.Add(new SomeFaultingType(), name: "Some faulting type");
            collection.Add<SomeNeverEndingType>(name: "Some never ending type");

            var aggregator = new Aggregator(collection);

            var result = await aggregator.Send<string>(42, timeout: TimeSpan.FromSeconds(2));

            Assert.NotNull(result);

            Assert.Equal(1, result.Completed.Count);
            Assert.Equal("Some delegate", result.Completed[0].Recipient.Name);
            Assert.Null(result.Completed[0].Recipient.Type);

            Assert.Equal(1, result.Faulted.Count);
            Assert.Equal("Some faulting type", result.Faulted[0].Recipient.Name);
            Assert.Equal(typeof(SomeFaultingType), result.Faulted[0].Recipient.Type);

            Assert.Equal(1, result.Incomplete.Count);
            Assert.Equal("Some never ending type", result.Incomplete[0].Recipient.Name);
            Assert.Equal(typeof(SomeNeverEndingType), result.Incomplete[0].Recipient.Type);
        }

        [Fact(Timeout = 5000)]
        public async Task Limit_completed_recipients()
        {
            var collection = new RecipientsCollection();

            // Three completing.
            collection.Add<SomeType>();
            collection.Add((int n) => n.ToString());
            collection.Add(async (int n) =>
            {
                await Task.Delay(1000);
                return n.ToString();
            });

            // Two never ending.
            collection.Add<SomeNeverEndingType>();
            collection.Add(new SomeNeverEndingType());

            var aggregator = new Aggregator(collection);

            {
                var (completed, faulted, aaa) = await aggregator.Send(42, new ScatterGatherOptions { Limit = 1 });

                Assert.Equal(1, completed.Count);
                Assert.Empty(faulted);
            }

            {
                var (completed, faulted, aaa) = await aggregator.Send(42, new ScatterGatherOptions { Limit = 3 });

                Assert.Equal(3, completed.Count);
                Assert.Empty(faulted);
            }
        }

        [Fact(Timeout = 5000)]
        public async Task Limit_returns_quickest_recipients()
        {
            var collection = new RecipientsCollection();

            // Three completing.
            collection.Add<SomeType>(name: "Quick 1");
            collection.Add((int n) => n.ToString(), name: "Quick 2");
            collection.Add(async (int n) =>
            {
                await Task.Delay(2000);
                return n.ToString();
            }, name: "Slowest");
            collection.Add(async (int n) =>
            {
                await Task.Delay(1000);
                return n.ToString();
            }, name: "Slow");

            // Two never ending.
            collection.Add<SomeNeverEndingType>();
            collection.Add(new SomeNeverEndingType());

            var aggregator = new Aggregator(collection);

            var (completed, faulted, aaa) = await aggregator.Send(42, new ScatterGatherOptions { Limit = 3 });

            Assert.Equal(3, completed.Count);
            Assert.Empty(faulted);

            Assert.Contains(completed, x => x.Recipient.Name == "Quick 1");
            Assert.Contains(completed, x => x.Recipient.Name == "Quick 2");
            Assert.Contains(completed, x => x.Recipient.Name == "Slow");
        }
    }
}
