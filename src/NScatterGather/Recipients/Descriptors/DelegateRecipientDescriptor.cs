using System;
using System.Threading.Tasks;

namespace NScatterGather.Recipients.Descriptors
{
    internal class DelegateRecipientDescriptor : IRecipientDescriptor
    {
        public Type RequestType { get; }

        public Type ResponseType { get; }

        public bool AcceptsCancellationToken { get; }

        public DelegateRecipientDescriptor(
            Type requestType,
            Type responseType,
            bool acceptsCancellationToken)
        {
            RequestType = requestType;
            ResponseType = responseType;
            AcceptsCancellationToken = acceptsCancellationToken;
        }

        public bool CanAccept(Type requestType, CollisionStrategy collisionStrategy)
        {
            var requestTypeMatches = TypesMatch(RequestType, requestType);
            return requestTypeMatches;
        }

        public bool CanReplyWith(Type requestType, Type responseType, CollisionStrategy collisionStrategy)
        {
            if (!TypesMatch(RequestType, requestType))
                return false;

            if (TypesMatch(ResponseType, responseType))
                return true;

            // The response type may be an awaitable!
            // (Task<TResponse>, ValueTask<TResponse>, ...)

            if (!ResponseType.IsAwaitableWithResult(out var awaitResponseType))
                return false;

            if (TypesMatch(awaitResponseType, responseType))
                return true;

            return false;
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
