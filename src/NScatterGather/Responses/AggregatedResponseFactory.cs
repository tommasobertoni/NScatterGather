using System.Collections.Generic;
using NScatterGather.Invocations;
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
                var recipientDescription = RecipientDescriptionFactory.CreateFrom(invocation.Recipient);

                if (invocation.CompletedSuccessfully)
                {
                    var completedInvocation = new CompletedInvocation<TResponse>(
                        recipientDescription,
                        invocation.Result,
                        invocation.Duration);

                    completed.Add(completedInvocation);
                }
                else if (invocation.Faulted)
                {
                    var faultedInvocation = new FaultedInvocation(
                        recipientDescription,
                        invocation.Exception,
                        invocation.Duration);

                    faulted.Add(faultedInvocation);
                }
                else
                {
                    incomplete.Add(new IncompleteInvocation(recipientDescription));
                }
            }

            return new AggregatedResponse<TResponse>(completed, faulted, incomplete);
        }
    }
}
