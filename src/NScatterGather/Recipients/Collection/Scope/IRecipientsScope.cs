using System;
using System.Collections.Generic;

namespace NScatterGather.Recipients.Collection.Scope
{
    internal interface IRecipientsScope
    {
        int RecipientsCount { get; }

        IReadOnlyList<Recipient> ListRecipientsAccepting(Type requestType);

        IReadOnlyList<Recipient> ListRecipientsReplyingWith(Type requestType, Type responseType);
    }
}
