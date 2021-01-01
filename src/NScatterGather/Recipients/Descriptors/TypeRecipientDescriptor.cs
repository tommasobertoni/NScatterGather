using System;
using NScatterGather.Inspection;

namespace NScatterGather.Recipients.Descriptors
{
    internal class TypeRecipientDescriptor : IRecipientDescriptor
    {
        private readonly TypeInspector _inspector;

        public TypeRecipientDescriptor(TypeInspector inspector)
        {
            _inspector = inspector;
        }

        public bool CanAccept(Type requestType, CollisionStrategy collisionStrategy)
        {
            var accepts = _inspector.HasMethodsAccepting(requestType, collisionStrategy);
            return accepts;
        }

        public bool CanReplyWith(Type requestType, Type responseType, CollisionStrategy collisionStrategy)
        {
            var repliesWith = _inspector.HasMethodsReturning(requestType, responseType, collisionStrategy);
            return repliesWith;
        }
    }
}
