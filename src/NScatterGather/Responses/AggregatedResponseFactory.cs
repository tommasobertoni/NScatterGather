using System;
using System.Collections.Generic;
using NScatterGather.Recipients;
using NScatterGather.Run;

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
                var recipientType = invocation.Recipient is InstanceRecipient ir ? ir.Type : null;

                if (invocation.CompletedSuccessfully)
                {
                    var completedInvocation = new CompletedInvocation<TResponse>(
                        invocation.Recipient.Name,
                        recipientType,
                        invocation.Result,
                        GetDuration(invocation));

                    completed.Add(completedInvocation);
                }
                else if (invocation.Faulted)
                {
                    var faultedInvocation = new FaultedInvocation(
                        invocation.Recipient.Name,
                        recipientType,
                        invocation.Exception,
                        GetDuration(invocation));

                    faulted.Add(faultedInvocation);
                }
                else
                    incomplete.Add(new IncompleteInvocation(invocation.Recipient.Name, recipientType));
            }

            return new AggregatedResponse<TResponse>(completed, faulted, incomplete);
        }

        private static TimeSpan GetDuration<TResponse>(
            RecipientRunner<TResponse> invocation)
        {
            if (invocation.StartedAt.HasValue && invocation.FinishedAt.HasValue)
                return invocation.FinishedAt.Value - invocation.StartedAt.Value;

            return default;
        }
    }
}
