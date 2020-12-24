using Xunit;

namespace NScatterGather.Recipients.Factories
{
    public class SingletonRecipientFactoryTests
    {
        [Fact]
        public void Factory_method_is_invoked_once()
        {
            var expectedInstance = new object();
            int count = 0;

            object factoryMethod()
            {
                count++;
                return expectedInstance;
            };

            var factory = new RecipientFactory(factoryMethod);
            var singletonFactory = new SingletonRecipientFactory(factory);

            _ = singletonFactory.Get();
            _ = singletonFactory.Get();
            var instance = singletonFactory.Get();

            Assert.Equal(expectedInstance, instance);
            Assert.Equal(1, count);
        }

        [Fact]
        public void Factory_can_accept_instance()
        {
            var expectedInstance = new object();

            var singletonFactory = new SingletonRecipientFactory(expectedInstance);
            var instance = singletonFactory.Get();

            Assert.Equal(expectedInstance, instance);
        }
    }
}
