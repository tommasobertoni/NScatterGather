using System;
using System.Linq;
using Xunit;

namespace NScatterGather.Recipients
{
    public class RecipientsCollectionTests
    {
        private readonly RecipientsCollection _collection;

        public RecipientsCollectionTests()
        {
            _collection = new RecipientsCollection();
        }

        [Fact]
        public void Can_add_generic_type()
        {
            _collection.Add<SomeType>();
        }

        [Fact]
        public void Can_add_generic_type_with_name()
        {
            _collection.Add<SomeType>(name: "My name is");
        }

        [Fact]
        public void Can_add_generic_type_with_lifetime()
        {
            _collection.Add<SomeType>(lifetime: Lifetime.Transient);
        }

        [Fact]
        public void Can_add_generic_type_with_factory_method()
        {
            _collection.Add(() => new SomeType());
        }

        [Fact]
        public void Error_if_factory_method_is_null()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _collection.Add((null as Func<SomeType>)!));
        }

        [Fact]
        public void Error_if_lifetime_is_not_valid()
        {
            Assert.Throws<ArgumentException>(() =>
                _collection.Add<SomeType>(lifetime: (Lifetime)42));
        }

        [Fact]
        public void Can_add_instance()
        {
            _collection.Add(new SomeType());
        }

        [Fact]
        public void Error_if_instance_is_null()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _collection.Add((null as object)!));
        }

        [Fact]
        public void Can_add_delegate()
        {
            _collection.Add((int n) => n.ToString());
        }

        [Fact]
        public void Error_if_delegate_is_null()
        {
            Func<int, string> func = null!;
            Assert.Throws<ArgumentNullException>(() => _collection.Add(func));
        }

        [Fact]
        public void Error_if_type_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => _collection.Add((null as Type)!));
        }

        [Fact]
        public void Can_add_recipient_instance()
        {
            _collection.Add(new SomeType());
        }

        [Fact]
        public void Fail_if_recipient_instance_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => _collection.Add((null as object)!));
        }

        [Fact]
        public void Can_create_scope()
        {
            var initialScope = _collection.CreateScope();

            _collection.Add<SomeType>();
            _collection.Add<SomeAsyncType>();
            _collection.Add<SomeFaultingType>();

            var nonEmptyScope = _collection.CreateScope();

            Assert.NotNull(initialScope);
            Assert.Equal(0, initialScope.RecipientsCount);
            Assert.NotNull(nonEmptyScope);
            Assert.Equal(3, nonEmptyScope.RecipientsCount);
        }

        //[Fact]
        //public void Recipients_accepting_request_can_be_found()
        //{
        //    var empty = _collection.ListRecipientsAccepting(typeof(int));
        //    Assert.Empty(empty);

        //    _collection.Add<SomeType>();

        //    var one = _collection.ListRecipientsAccepting(typeof(int))
        //        .Where(x => x is TypeRecipient)
        //        .Cast<TypeRecipient>()
        //        .ToList();

        //    Assert.Single(one);
        //    Assert.Equal(typeof(SomeType), one.First().Type);

        //    _collection.Add<SomeEchoType>();

        //    var two = _collection.ListRecipientsAccepting(typeof(int))
        //        .Where(x => x is TypeRecipient)
        //        .Cast<TypeRecipient>()
        //        .ToList();

        //    Assert.Equal(2, two.Count);
        //    Assert.Contains(typeof(SomeType), two.Select(x => x.Type));
        //    Assert.Contains(typeof(SomeEchoType), two.Select(x => x.Type));

        //    _collection.Add<SomeDifferentType>();
        //    var stillTwo = _collection.ListRecipientsAccepting(typeof(int));
        //    Assert.Equal(2, stillTwo.Count);

        //    var differentOne = _collection.ListRecipientsAccepting(typeof(string))
        //        .Where(x => x is TypeRecipient)
        //        .Cast<TypeRecipient>()
        //        .ToList();

        //    Assert.Single(differentOne);
        //    Assert.Equal(typeof(SomeDifferentType), differentOne.First().Type);
        //}

        //[Fact]
        //public void Recipients_returning_response_can_be_found()
        //{
        //    var empty = _collection.ListRecipientsReplyingWith(typeof(int), typeof(string));
        //    Assert.Empty(empty);

        //    _collection.Add<SomeType>();

        //    var one = _collection.ListRecipientsReplyingWith(typeof(int), typeof(string))
        //        .Where(x => x is TypeRecipient)
        //        .Cast<TypeRecipient>()
        //        .ToList();

        //    Assert.Single(one);
        //    Assert.Equal(typeof(SomeType), one.First().Type);

        //    _collection.Add<SomeEchoType>();

        //    var two = _collection.ListRecipientsReplyingWith(typeof(int), typeof(string))
        //        .Where(x => x is TypeRecipient)
        //        .Cast<TypeRecipient>()
        //        .ToList();

        //    Assert.Equal(2, two.Count);
        //    Assert.Contains(typeof(SomeType), two.Select(x => x.Type));
        //    Assert.Contains(typeof(SomeEchoType), two.Select(x => x.Type));

        //    _collection.Add<SomeDifferentType>();
        //    var stillTwo = _collection.ListRecipientsReplyingWith(typeof(int), typeof(string));
        //    Assert.Equal(2, stillTwo.Count);

        //    var differentOne = _collection.ListRecipientsReplyingWith(typeof(string), typeof(int))
        //        .Where(x => x is TypeRecipient)
        //        .Cast<TypeRecipient>()
        //        .ToList();

        //    Assert.Single(differentOne);
        //    Assert.Equal(typeof(SomeDifferentType), differentOne.First().Type);
        //}

        //[Fact]
        //public void Recipients_with_request_collisions_are_ignored()
        //{
        //    _collection.Add<SomeType>();
        //    _collection.Add<SomeCollidingType>();

        //    var onlyNonCollidingType = _collection.ListRecipientsAccepting(typeof(int))
        //        .Where(x => x is TypeRecipient)
        //        .Cast<TypeRecipient>()
        //        .ToList();

        //    Assert.Single(onlyNonCollidingType);
        //    Assert.Equal(typeof(SomeType), onlyNonCollidingType.First().Type);
        //}

        //[Fact]
        //public void Recipients_with_request_and_response_collisions_are_ignored()
        //{
        //    _collection.Add<SomeType>();
        //    _collection.Add<SomeCollidingType>();

        //    var onlyNonCollidingType = _collection.ListRecipientsReplyingWith(typeof(int), typeof(string))
        //        .Where(x => x is TypeRecipient)
        //        .Cast<TypeRecipient>()
        //        .ToList();

        //    Assert.Single(onlyNonCollidingType);
        //    Assert.Equal(typeof(SomeType), onlyNonCollidingType.First().Type);
        //}

        //[Fact]
        //public void Collisions_can_be_resolved_via_return_type()
        //{
        //    _collection.Add<SomeType>();
        //    _collection.Add<AlmostCollidingType>();

        //    var two = _collection.ListRecipientsReplyingWith(typeof(int), typeof(string));
        //    Assert.Equal(2, two.Count);
        //}

        //[Fact]
        //public void Can_be_cloned()
        //{
        //    _collection.Add<SomeType>();
        //    _collection.Add<SomeEchoType>();

        //    Assert.NotEmpty(_collection.RecipientTypes);

        //    var clone = _collection.Clone();

        //    foreach (var type in _collection.RecipientTypes)
        //        Assert.Contains(type, clone.RecipientTypes);
        //}

        //[Fact]
        //public void Recipients_can_have_a_name()
        //{
        //    _collection.Add<SomeType>(name: "Some type");
        //    _collection.Add(new SomeEchoType(), "Some other type");
        //    _collection.Add((int n) => n.ToString(), "Delegate recipient");

        //    Assert.Equal(3, _collection.Recipients.Count);

        //    foreach (var recipient in _collection.Recipients)
        //    {
        //        if (recipient is TypeRecipient tr)
        //        {
        //            if (tr.Type == typeof(SomeType))
        //                Assert.Equal("Some type", tr.Name);
        //            else if (tr.Type == typeof(SomeEchoType))
        //                Assert.Equal("Some other type", tr.Name);
        //            else
        //                throw new Xunit.Sdk.XunitException();
        //        }
        //        else if (recipient is DelegateRecipient dr)
        //        {
        //            Assert.Equal("Delegate recipient", dr.Name);
        //        }
        //        else
        //        {
        //            throw new Xunit.Sdk.XunitException();
        //        }
        //    }
        //}
    }
}
