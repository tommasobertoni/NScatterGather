using System;

namespace NScatterGather.Recipients.Descriptors
{
    internal interface IRecipientDescriptor
    {
        bool CanAccept(Type requestType);

        bool CanReplyWith(Type requestType, Type responseType);
    }
}
