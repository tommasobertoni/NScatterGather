using System.Collections.Generic;
using System.Linq;
using NScatterGather.Invocations;
using NScatterGather.Recipients.Run;

namespace NScatterGather.Responses
{
    internal class AggregatedResponseFactory
    {
        public static AggregatedResponse<TResponse> CreateFrom<TResponse>(
            IEnumerable<RecipientRunner<TResponse>> runners,
            ScatterGatherOptions options)
        {
            var completed = new List<CompletedInvocation<TResponse>>();
            var faulted = new List<FaultedInvocation>();
            var incomplete = new List<IncompleteInvocation>();

            foreach (var runner in runners)
            {
                var recipientDescription = RecipientDescriptionFactory.CreateFrom(runner.Recipient);

                if (runner.HasCompletedSuccessfully)
                {
                    var completedInvocation = new CompletedInvocation<TResponse>(
                        recipientDescription,
                        runner.Result,
                        runner.Duration);

                    completed.Add(completedInvocation);
                }
                else if (runner.HasFaulted)
                {
                    var faultedInvocation = new FaultedInvocation(
                        recipientDescription,
                        runner.Exception,
                        runner.Duration);

                    faulted.Add(faultedInvocation);
                }
                else
                {
                    incomplete.Add(new IncompleteInvocation(recipientDescription));
                }
            }

            if (options.Limit.HasValue)
            {
                completed = completed.OrderBy(x => x.Duration).Take(options.Limit.Value).ToList();
            }

            return new AggregatedResponse<TResponse>(completed, faulted, incomplete);
        }
    }
}
