using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NScatterGather
{
    public class AggregatorTests
    {
        private readonly Aggregator _aggregator;

        public AggregatorTests()
        {
            var collection = new RecipientsCollection();
            collection.Add<SomeType>();
            collection.Add<SomeAsyncType>();
            collection.Add<SomePossiblyAsyncType>();
            collection.Add<SomeCollidingType>();
            collection.Add<SomeFaultingType>();
            collection.Add<SomeNeverEndingType>();

            _aggregator = new Aggregator(collection);
        }

        [Fact(Timeout = 5_000)]
        public async Task Sends_request_and_aggregates_responses()
        {
            var result = await _aggregator.Send(42, timeout: TimeSpan.FromSeconds(2));

            Assert.NotNull(result);
            Assert.Equal(3, result.Completed.Count);
            Assert.Contains(typeof(SomeType), result.Completed.Select(x => x.Recipient.Type));
            Assert.Contains(typeof(SomeAsyncType), result.Completed.Select(x => x.Recipient.Type));
            Assert.Contains(typeof(SomePossiblyAsyncType), result.Completed.Select(x => x.Recipient.Type));

            Assert.Single(result.Faulted);
            Assert.Contains(typeof(SomeFaultingType), result.Faulted.Select(x => x.Recipient.Type));

            Assert.Single(result.Incomplete);
            Assert.Contains(typeof(SomeNeverEndingType), result.Incomplete.Select(x => x.Recipient.Type));
        }

        [Fact(Timeout = 5_000)]
        public async Task Receives_expected_response_types()
        {
            var result = await _aggregator.Send<string>(42, timeout: TimeSpan.FromSeconds(2));

            Assert.NotNull(result);
            Assert.Equal(3, result.Completed.Count);
            Assert.Contains(typeof(SomeType), result.Completed.Select(x => x.Recipient.Type));
            Assert.Contains(typeof(SomeAsyncType), result.Completed.Select(x => x.Recipient.Type));
            Assert.Contains(typeof(SomePossiblyAsyncType), result.Completed.Select(x => x.Recipient.Type));

            Assert.Single(result.Faulted);
            Assert.Contains(typeof(SomeFaultingType), result.Faulted.Select(x => x.Recipient.Type));

            Assert.Single(result.Incomplete);
            Assert.Contains(typeof(SomeNeverEndingType), result.Incomplete.Select(x => x.Recipient.Type));
        }

        [Fact]
        public async Task Responses_expose_the_recipient_name_and_type()
        {
            var collection = new RecipientsCollection();
            collection.Add((int n) => n.ToString(), name: "Some delegate");
            collection.Add(new SomeFaultingType(), name: "Some faulting type");
            collection.Add<SomeNeverEndingType>(name: "Some never ending type");

            var localAggregator = new Aggregator(collection);
            var result = await localAggregator.Send<string>(42, timeout: TimeSpan.FromSeconds(2));

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

        [Fact]
        public void Error_if_request_is_null()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _aggregator.Send((null as object)!));
            Assert.ThrowsAsync<ArgumentNullException>(() => _aggregator.Send<int>((null as object)!));
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        public async Task Recipients_are_cancelled_after_timeout()
        {
            var collection = new RecipientsCollection();
            collection.Add<SomeType>();
            collection.Add<SomeAlmostNeverEndingType>();

            var aggregator = new Aggregator(collection);

            var response = await aggregator.Send(42, TimeSpan.FromSeconds(2));
            Assert.Equal(2, response.Completed.Count);
            Assert.Empty(response.Incomplete);
        }
    }
}
