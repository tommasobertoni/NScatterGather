using System;

namespace NScatterGather.Recipients.Descriptors
{
    internal interface IRecipientDescriptor
    {
        bool CanAccept(Type requestType, CollisionStrategy collisionStrategy);

        bool CanReplyWith(Type requestType, Type responseType, CollisionStrategy collisionStrategy);
    }
}
