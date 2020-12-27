using System;

namespace NScatterGather.Recipients.Factories
{
    internal class SingletonRecipientFactory : IRecipientFactory
    {
        private readonly IRecipientFactory? _anotherFactory;
        private readonly Lazy<object> _lazyInstance;

        public SingletonRecipientFactory(object instance)
        {
#if NETSTANDARD2_0
            _lazyInstance = new Lazy<object>(() => instance);
#else
            _lazyInstance = new Lazy<object>(instance);
#endif
        }

        public SingletonRecipientFactory(IRecipientFactory anotherFactory)
        {
            _anotherFactory = anotherFactory;
            _lazyInstance = new Lazy<object>(anotherFactory.Get);
        }

        public object Get() => _lazyInstance.Value;

        public IRecipientFactory Clone() =>
            _anotherFactory is null
            ? new SingletonRecipientFactory(_lazyInstance.Value)
            : new SingletonRecipientFactory(_anotherFactory.Clone());
    }
}
