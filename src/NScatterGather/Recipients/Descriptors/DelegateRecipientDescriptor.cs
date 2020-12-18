using System;

namespace NScatterGather.Recipients.Descriptors
{
    internal class DelegateRecipientDescriptor : IRecipientDescriptor
    {
        public Type RequestType { get; }

        public Type ResponseType { get; }

        public DelegateRecipientDescriptor(Type requestType, Type responseType)
        {
            RequestType = requestType;
            ResponseType = responseType;
        }

        public bool CanAccept(Type requestType)
        {
            var requestTypeMatches = TypesMatch(RequestType, requestType);
            return requestTypeMatches;
        }

        public bool CanReplyWith(Type requestType, Type responseType)
        {
            var requestAndResponseMatch =
                TypesMatch(RequestType, requestType) &&
                TypesMatch(ResponseType, responseType);

            return requestAndResponseMatch;
        }

        private bool TypesMatch(Type target, Type actual)
        {
            if (target == actual)
                return true;

            var nonNullableType = Nullable.GetUnderlyingType(target);
            if (nonNullableType is not null && nonNullableType == actual)
                return true;

            return false;
        }
    }
}
