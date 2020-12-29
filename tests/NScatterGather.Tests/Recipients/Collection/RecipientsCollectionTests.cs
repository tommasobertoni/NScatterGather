using System;
using Xunit;

namespace NScatterGather.Recipients.Collection
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
            _collection.Add<SomeType>(lifetime: Lifetime.Scoped);
            _collection.Add<SomeType>(lifetime: Lifetime.Singleton);
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
        public void Error_if_no_parameterless_constructor()
        {
            Assert.Throws<ArgumentException>(() => _collection.Add<SomeTypeWithConstructor>());
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

        [Fact]
        public void Recipients_count_is_visible()
        {
            Assert.Equal(0, _collection.RecipientsCount);

            _collection.Add<SomeType>();
            _collection.Add(new SomeAsyncType());
            _collection.Add((int n) => n);

            Assert.Equal(3, _collection.RecipientsCount);
        }
    }
}
