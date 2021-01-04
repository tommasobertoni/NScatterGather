using System.Collections.Generic;
using NScatterGather.Recipients;
using NScatterGather.Recipients.Run;

namespace NScatterGather.Responses
{
    internal class AggregatedResponseFactory
    {
        public static AggregatedResponse<TResponse> CreateFrom<TResponse>(
            IEnumerable<RecipientRunner<TResponse>> invocations)
        {
            var completed = new List<CompletedInvocation<TResponse>>();
            var faulted = new List<FaultedInvocation>();
            var incomplete = new List<IncompleteInvocation>();

            foreach (var invocation in invocations)
            {
                var recipient = invocation.Recipient;
                var recipientType = recipient is TypeRecipient ir ? ir.Type : null;

                if (invocation.CompletedSuccessfully)
                {
                    var completedInvocation = new CompletedInvocation<TResponse>(
                        recipient.Id,
                        recipient.Name,
                        recipientType,
                        invocation.Result,
                        invocation.Duration);

                    completed.Add(completedInvocation);
                }
                else if (invocation.Faulted)
                {
                    var faultedInvocation = new FaultedInvocation(
                        recipient.Id,
                        recipient.Name,
                        recipientType,
                        invocation.Exception,
                        invocation.Duration);

                    faulted.Add(faultedInvocation);
                }
                else
                    incomplete.Add(new IncompleteInvocation(
                        recipient.Id,
                        recipient.Name,
                        recipientType));
            }

            return new AggregatedResponse<TResponse>(completed, faulted, incomplete);
        }
    }
}
