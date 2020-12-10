using System;

namespace NScatterGather
{
    public class ConflictException : Exception
    {
        public Type RecipientType { get; }

        public Type RequestType { get; }

        public Type? ResponseType { get; }

        internal ConflictException(
            Type recipientType,
            Type requestType,
            Type? responseType = null)
            : base(CreateMessage(recipientType, requestType, responseType))
        {
            RecipientType = recipientType;
            RequestType = requestType;
            ResponseType = responseType;
        }

        private static string CreateMessage(
            Type recipientType,
            Type requestType,
            Type? responseType)
        {
            if (responseType is null)
            {
                return
                    $"Type '{recipientType.Name}' has too many " +
                    $"matching methods accepting '{requestType.Name}'.";
            }
            else
            {
                return
                    $"Type '{recipientType.Name}' has too many matching methods " +
                    $"accepting '{requestType.Name}' and returning '{responseType.Name}'.";
            }
        }
    }
}
