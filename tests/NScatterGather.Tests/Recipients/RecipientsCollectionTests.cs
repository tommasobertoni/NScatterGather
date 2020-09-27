﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
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
            var mock = new Mock<ILogger<RecipientsCollection>>();
            _collection = new RecipientsCollection(mock.Object);
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
        public void Error_if_type_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => _collection.Add(null!));
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
            var empty = _collection.ListRecipientsAccepting<int>();
            Assert.Empty(empty);

            _collection.Add<SomeType>();
            var one = _collection.ListRecipientsAccepting<int>();
            Assert.Single(one);
            Assert.Equal(typeof(SomeType), one.First().Type);

            _collection.Add<SomeOtherType>();
            var two = _collection.ListRecipientsAccepting<int>();
            Assert.Equal(2, two.Count);
            Assert.Contains(typeof(SomeType), two.Select(x => x.Type));
            Assert.Contains(typeof(SomeOtherType), two.Select(x => x.Type));

            _collection.Add<SomeDifferentType>();
            var stillTwo = _collection.ListRecipientsAccepting<int>();
            Assert.Equal(2, stillTwo.Count);

            var differentOne = _collection.ListRecipientsAccepting<string>();
            Assert.Single(differentOne);
            Assert.Equal(typeof(SomeDifferentType), differentOne.First().Type);
        }

        [Fact]
        public void Recipients_returning_response_can_be_found()
        {
            var empty = _collection.ListRecipientsReplyingWith<int, string>();
            Assert.Empty(empty);

            _collection.Add<SomeType>();
            var one = _collection.ListRecipientsReplyingWith<int, string>();
            Assert.Single(one);
            Assert.Equal(typeof(SomeType), one.First().Type);

            _collection.Add<SomeOtherType>();
            var two = _collection.ListRecipientsReplyingWith<int, string>();
            Assert.Equal(2, two.Count);
            Assert.Contains(typeof(SomeType), two.Select(x => x.Type));
            Assert.Contains(typeof(SomeOtherType), two.Select(x => x.Type));

            _collection.Add<SomeDifferentType>();
            var stillTwo = _collection.ListRecipientsReplyingWith<int, string>();
            Assert.Equal(2, stillTwo.Count);

            var differentOne = _collection.ListRecipientsReplyingWith<string, int>();
            Assert.Single(differentOne);
            Assert.Equal(typeof(SomeDifferentType), differentOne.First().Type);
        }

        [Fact]
        public void Recipients_with_request_collisions_are_ignored()
        {
            _collection.Add<SomeType>();
            _collection.Add<CollidingType>();

            var onlyNonCollidingType = _collection.ListRecipientsAccepting<int>();
            Assert.Single(onlyNonCollidingType);
            Assert.Equal(typeof(SomeType), onlyNonCollidingType.First().Type);
        }

        [Fact]
        public void Recipients_with_request_and_response_collisions_are_ignored()
        {
            _collection.Add<SomeType>();
            _collection.Add<CollidingType>();

            var onlyNonCollidingType = _collection.ListRecipientsReplyingWith<int, string>();
            Assert.Single(onlyNonCollidingType);
            Assert.Equal(typeof(SomeType), onlyNonCollidingType.First().Type);
        }

        [Fact]
        public void Collisions_can_be_resolved_via_return_type()
        {
            _collection.Add<SomeType>();
            _collection.Add<AlmostCollidingType>();

            var two = _collection.ListRecipientsReplyingWith<int, string>();
            Assert.Equal(2, two.Count);
        }
    }
}