using Xunit;

namespace NScatterGather.Recipients.Factories
{
    public class RecipientFactoryTests
    {
        [Fact]
        public void Factory_method_is_invoked()
        {
            var expectedInstance = new object();
            int count = 0;

            object factoryMethod()
            {
                count++;
                return expectedInstance;
            };

            var factory = new RecipientFactory(factoryMethod);

            _ = factory.Get();
            _ = factory.Get();
            var instance = factory.Get();

            Assert.Equal(expectedInstance, instance);
            Assert.Equal(3, count);
        }

        [Fact]
        public void Can_be_cloned()
        {
            int count = 0;

            object factoryMethod()
            {
                count++;
                return new object();
            };

            var factory = new RecipientFactory(factoryMethod);
            var clone = factory.Clone();

            Assert.IsType<RecipientFactory>(clone);

            _ = factory.Get();
            _ = clone.Get();
            Assert.Equal(2, count);

            _ = factory.Get();
            _ = clone.Get();
            Assert.Equal(4, count);
        }
    }
}
