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
    }
}
