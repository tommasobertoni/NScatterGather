using System;
using NScatterGather.Inspection;

namespace NScatterGather.Recipients.Descriptors
{
    internal class TypeRecipientDescriptor : IRecipientDescriptor
    {
        private readonly TypeInspector _inspector;

        public TypeRecipientDescriptor(Type type)
            : this(new TypeInspector(type))
        {
        }

        public TypeRecipientDescriptor(TypeInspector inspector)
        {
            _inspector = inspector;
        }

        public bool CanAccept(Type requestType)
        {
            var accepts = _inspector.HasMethodAccepting(requestType);
            return accepts;
        }

        public bool CanReplyWith(Type requestType, Type responseType)
        {
            var repliesWith = _inspector.HasMethodReturning(requestType, responseType);
            return repliesWith;
        }
    }
}
