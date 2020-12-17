using System;

namespace NScatterGather.Recipients.Factories
{
    internal class RecipientFactory : IRecipientFactory
    {
        private readonly Func<object> _factory;

        public RecipientFactory(Func<object> factory)
        {
            _factory = factory;
        }

        public object Get() => _factory();
    }
}
