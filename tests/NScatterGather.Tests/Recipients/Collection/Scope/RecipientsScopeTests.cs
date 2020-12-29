using System.Linq;
using NScatterGather.Inspection;
using Xunit;

namespace NScatterGather.Recipients.Collection.Scope
{
    public class RecipientsScopeTests
    {
        [Fact]
        public void Recipients_can_be_added()
        {
            var scope = new RecipientsScope();

            Assert.Equal(0, scope.RecipientsCount);

            scope.AddRange(new[] { DelegateRecipient.Create((int n) => n, name: null) });

            Assert.Equal(1, scope.RecipientsCount);
        }

        [Fact]
        public void Recipients_accepting_request_can_be_found()
        {
            var scope = new RecipientsScope();

            var empty = scope.ListRecipientsAccepting(typeof(int));
            Assert.Empty(empty);

            scope.AddTypeRecipient<SomeType>();

            var one = scope.ListRecipientsAccepting(typeof(int))
                .Where(x => x is TypeRecipient)
                .Cast<TypeRecipient>()
                .ToList();

            Assert.Single(one);
            Assert.Equal(typeof(SomeType), one.First().Type);

            scope.AddTypeRecipient<SomeEchoType>();

            var two = scope.ListRecipientsAccepting(typeof(int))
                .Where(x => x is TypeRecipient)
                .Cast<TypeRecipient>()
                .ToList();

            Assert.Equal(2, two.Count);
            Assert.Contains(typeof(SomeType), two.Select(x => x.Type));
            Assert.Contains(typeof(SomeEchoType), two.Select(x => x.Type));

            scope.AddTypeRecipient<SomeDifferentType>();

            var stillTwo = scope.ListRecipientsAccepting(typeof(int));
            Assert.Equal(2, stillTwo.Count);

            var differentOne = scope.ListRecipientsAccepting(typeof(string))
                .Where(x => x is TypeRecipient)
                .Cast<TypeRecipient>()
                .ToList();

            Assert.Single(differentOne);
            Assert.Equal(typeof(SomeDifferentType), differentOne.First().Type);
        }

        [Fact]
        public void Recipients_returning_response_can_be_found()
        {
            var scope = new RecipientsScope();

            var empty = scope.ListRecipientsReplyingWith(typeof(int), typeof(string));
            Assert.Empty(empty);

            scope.AddTypeRecipient<SomeType>();

            var one = scope.ListRecipientsReplyingWith(typeof(int), typeof(string))
                .Where(x => x is TypeRecipient)
                .Cast<TypeRecipient>()
                .ToList();

            Assert.Single(one);
            Assert.Equal(typeof(SomeType), one.First().Type);

            scope.AddTypeRecipient<SomeEchoType>();

            var two = scope.ListRecipientsReplyingWith(typeof(int), typeof(string))
                .Where(x => x is TypeRecipient)
                .Cast<TypeRecipient>()
                .ToList();

            Assert.Equal(2, two.Count);
            Assert.Contains(typeof(SomeType), two.Select(x => x.Type));
            Assert.Contains(typeof(SomeEchoType), two.Select(x => x.Type));

            scope.AddTypeRecipient<SomeDifferentType>();

            var stillTwo = scope.ListRecipientsReplyingWith(typeof(int), typeof(string));
            Assert.Equal(2, stillTwo.Count);

            var differentOne = scope.ListRecipientsReplyingWith(typeof(string), typeof(int))
                .Where(x => x is TypeRecipient)
                .Cast<TypeRecipient>()
                .ToList();

            Assert.Single(differentOne);
            Assert.Equal(typeof(SomeDifferentType), differentOne.First().Type);
        }

        [Fact]
        public void Recipients_with_request_collisions_are_ignored()
        {
            CollisionException? collisionException = null;

            var scope = new RecipientsScope();
            scope.OnCollision += (e) => collisionException = e;

            scope.AddTypeRecipient<SomeType>();
            scope.AddTypeRecipient<SomeCollidingType>();

            var onlyNonCollidingType = scope.ListRecipientsAccepting(typeof(int))
                .Where(x => x is TypeRecipient)
                .Cast<TypeRecipient>()
                .ToList();

            Assert.Single(onlyNonCollidingType);
            Assert.Equal(typeof(SomeType), onlyNonCollidingType.First().Type);

            Assert.NotNull(collisionException);
            Assert.NotNull(collisionException!.Message);
            Assert.Equal(typeof(SomeCollidingType), collisionException!.RecipientType);
            Assert.Equal(typeof(int), collisionException!.RequestType);
            Assert.Null(collisionException!.ResponseType);
        }

        [Fact]
        public void Recipients_with_request_and_response_collisions_are_ignored()
        {
            CollisionException? collisionException = null;

            var scope = new RecipientsScope();
            scope.OnCollision += (e) => collisionException = e;

            scope.AddTypeRecipient<SomeType>();
            scope.AddTypeRecipient<SomeCollidingType>();

            var onlyNonCollidingType = scope.ListRecipientsReplyingWith(typeof(int), typeof(string))
                .Where(x => x is TypeRecipient)
                .Cast<TypeRecipient>()
                .ToList();

            Assert.Single(onlyNonCollidingType);
            Assert.Equal(typeof(SomeType), onlyNonCollidingType.First().Type);

            Assert.NotNull(collisionException);
            Assert.NotNull(collisionException!.Message);
            Assert.Equal(typeof(SomeCollidingType), collisionException!.RecipientType);
            Assert.Equal(typeof(int), collisionException!.RequestType);
            Assert.Equal(typeof(string), collisionException!.ResponseType);
        }

        [Fact]
        public void Collisions_can_be_resolved_via_return_type()
        {
            var scope = new RecipientsScope();
            scope.OnCollision += _ => Assert.False(true, "No collisions should be detected");

            scope.AddTypeRecipient<SomeType>();
            scope.AddTypeRecipient<AlmostCollidingType>();

            var two = scope.ListRecipientsReplyingWith(typeof(int), typeof(string));
            Assert.Equal(2, two.Count);
        }
    }
}
