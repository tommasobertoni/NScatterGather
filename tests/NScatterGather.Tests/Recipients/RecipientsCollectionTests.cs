using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NScatterGather.Recipients
{
    public class RecipientsCollectionTests
    {
        class SomeType
        {
            public string Echo(int n) => n.ToString();
        }

        class SomeOtherType
        {
            public Task<string> Echo(int n) => Task.FromResult(n.ToString());
        }

        class SomeDifferentType
        {
            public int SomethingElse(string s) => s.Length;
        }

        class CollidingType
        {
            public string Do(int n) => n.ToString();

            public string DoDifferently(int n) => $"n";
        }

        class AlmostCollidingType
        {
            public string Do(int n) => n.ToString();

            public long DoDifferently(int n) => (long)n;
        }

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
        public void Can_add_type()
        {
            _collection.Add(typeof(SomeType));
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
        public void Recipients_accepting_request_can_be_found()
        {
            var empty = _collection.ListRecipientsAccepting(typeof(int));
            Assert.Empty(empty);

            _collection.Add<SomeType>();

            var one = _collection.ListRecipientsAccepting(typeof(int))
                .Where(x => x is InstanceRecipient)
                .Cast<InstanceRecipient>()
                .ToList();

            Assert.Single(one);
            Assert.Equal(typeof(SomeType), one.First().Type);

            _collection.Add<SomeOtherType>();

            var two = _collection.ListRecipientsAccepting(typeof(int))
                .Where(x => x is InstanceRecipient)
                .Cast<InstanceRecipient>()
                .ToList();

            Assert.Equal(2, two.Count);
            Assert.Contains(typeof(SomeType), two.Select(x => x.Type));
            Assert.Contains(typeof(SomeOtherType), two.Select(x => x.Type));

            _collection.Add<SomeDifferentType>();
            var stillTwo = _collection.ListRecipientsAccepting(typeof(int));
            Assert.Equal(2, stillTwo.Count);

            var differentOne = _collection.ListRecipientsAccepting(typeof(string))
                .Where(x => x is InstanceRecipient)
                .Cast<InstanceRecipient>()
                .ToList();

            Assert.Single(differentOne);
            Assert.Equal(typeof(SomeDifferentType), differentOne.First().Type);
        }

        [Fact]
        public void Recipients_returning_response_can_be_found()
        {
            var empty = _collection.ListRecipientsReplyingWith(typeof(int), typeof(string));
            Assert.Empty(empty);

            _collection.Add<SomeType>();

            var one = _collection.ListRecipientsReplyingWith(typeof(int), typeof(string))
                .Where(x => x is InstanceRecipient)
                .Cast<InstanceRecipient>()
                .ToList();

            Assert.Single(one);
            Assert.Equal(typeof(SomeType), one.First().Type);

            _collection.Add<SomeOtherType>();

            var two = _collection.ListRecipientsReplyingWith(typeof(int), typeof(string))
                .Where(x => x is InstanceRecipient)
                .Cast<InstanceRecipient>()
                .ToList();

            Assert.Equal(2, two.Count);
            Assert.Contains(typeof(SomeType), two.Select(x => x.Type));
            Assert.Contains(typeof(SomeOtherType), two.Select(x => x.Type));

            _collection.Add<SomeDifferentType>();
            var stillTwo = _collection.ListRecipientsReplyingWith(typeof(int), typeof(string));
            Assert.Equal(2, stillTwo.Count);

            var differentOne = _collection.ListRecipientsReplyingWith(typeof(string), typeof(int))
                .Where(x => x is InstanceRecipient)
                .Cast<InstanceRecipient>()
                .ToList();

            Assert.Single(differentOne);
            Assert.Equal(typeof(SomeDifferentType), differentOne.First().Type);
        }

        [Fact]
        public void Recipients_with_request_collisions_are_ignored()
        {
            _collection.Add<SomeType>();
            _collection.Add<CollidingType>();

            var onlyNonCollidingType = _collection.ListRecipientsAccepting(typeof(int))
                .Where(x => x is InstanceRecipient)
                .Cast<InstanceRecipient>()
                .ToList();

            Assert.Single(onlyNonCollidingType);
            Assert.Equal(typeof(SomeType), onlyNonCollidingType.First().Type);
        }

        [Fact]
        public void Recipients_with_request_and_response_collisions_are_ignored()
        {
            _collection.Add<SomeType>();
            _collection.Add<CollidingType>();

            var onlyNonCollidingType = _collection.ListRecipientsReplyingWith(typeof(int), typeof(string))
                .Where(x => x is InstanceRecipient)
                .Cast<InstanceRecipient>()
                .ToList();

            Assert.Single(onlyNonCollidingType);
            Assert.Equal(typeof(SomeType), onlyNonCollidingType.First().Type);
        }

        [Fact]
        public void Collisions_can_be_resolved_via_return_type()
        {
            _collection.Add<SomeType>();
            _collection.Add<AlmostCollidingType>();

            var two = _collection.ListRecipientsReplyingWith(typeof(int), typeof(string));
            Assert.Equal(2, two.Count);
        }

        [Fact]
        public void Can_be_cloned()
        {
            _collection.Add<SomeType>();
            _collection.Add<SomeOtherType>();

            Assert.NotEmpty(_collection.RecipientTypes);

            var clone = _collection.Clone();

            foreach (var type in _collection.RecipientTypes)
                Assert.Contains(type, clone.RecipientTypes);
        }

        [Fact]
        public void Recipients_can_have_a_name()
        {
            _collection.Add<SomeType>("Some type");
            _collection.Add(new SomeOtherType(), "Some other type");
            _collection.Add((int n) => n.ToString(), "Delegate recipient");

            Assert.Equal(3, _collection.Recipients.Count);

            foreach (var recipient in _collection.Recipients)
            {
                if (recipient is InstanceRecipient ir)
                {
                    if (ir.Type == typeof(SomeType))
                        Assert.Equal("Some type", ir.Name);
                    else if (ir.Type == typeof(SomeOtherType))
                        Assert.Equal("Some other type", ir.Name);
                    else
                        throw new Xunit.Sdk.XunitException();
                }
                else if (recipient is DelegateRecipient dr)
                {
                    Assert.Equal("Delegate recipient", dr.Name);
                }
                else
                {
                    throw new Xunit.Sdk.XunitException();
                }
            }
        }
    }
}
