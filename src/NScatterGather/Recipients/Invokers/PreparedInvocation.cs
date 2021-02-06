using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using IsAwaitable.Analysis;

namespace NScatterGather.Recipients.Invokers
{
    internal class PreparedInvocation<TResult>
    {
        public bool AcceptedCancellationToken { get; }

        private readonly Func<object?> _invocation;

        public PreparedInvocation(
            Func<object?> invocation,
            bool acceptedCancellationToken)
        {
            _invocation = invocation;
            AcceptedCancellationToken = acceptedCancellationToken;
        }

        public async Task<TResult> Execute()
        {
            try
            {
                var invocationResult = _invocation();
                var completedResult = await UnwrapAsyncResult(invocationResult).ConfigureAwait(false);
                return (TResult)completedResult!; // The response type will match TResponse, even on structs.
            }
            catch (TargetInvocationException tIEx) when (tIEx.InnerException is not null)
            {
                ExceptionDispatchInfo.Capture(tIEx.InnerException).Throw();
                return default!; // Unreachable
            }
        }

        private static async Task<object?> UnwrapAsyncResult(object? response)
        {
            if (response is null)
                return response;

            if (!response.IsAwaitableWithResult())
                return response!;

            // Fun fact: when the invoked method returns (asynchronously)
            // a non-public type (e.g. anonymous, internal...), the
            // 'dynamic await' approach will fail with the following exception:
            // `RuntimeBinderException: Cannot implicitly convert type 'void' to 'object'`.
            //
            // This happens because the dynamic binding treats them as the closest public
            // inherited type it knows about.
            // https://stackoverflow.com/questions/31778977/why-cant-i-access-properties-of-an-anonymous-type-returned-from-a-function-via/31779069#31779069
            //
            // If the method returned a `Task<T>`, the result of the dynamic binding
            // will be a `Task`, i.e. the closest public inherited type.
            //
            // Since `Task` is awaitable, but has no result, the runtime will try to:
            // ```
            // dynamic awaiter = ((dynamic)response).GetAwaiter();
            // var result = awaiter.GetResult();
            // ```
            // but `GetResult()` returns `void`! And that's why the exception.
            //
            // Solution: treat it like a Task, then extract the result via method invocation.

            await (dynamic)response;

            // At this point the task is completed.

            // The AwaitableDescription is not null since the response was
            // evaluated with `IsAwaitableWithResult`
            var description = Awaitable.Describe(response)!;

            var awaiter = description.GetAwaiterMethod.Invoke(response, null);
            var awaitedResult = description.AwaiterDescriptor.GetResultMethod.Invoke(awaiter, null);
            return awaitedResult;
        }
    }
}
