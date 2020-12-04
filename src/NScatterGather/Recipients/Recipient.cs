using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using IsAwaitable.Analysis;

namespace NScatterGather.Recipients
{
    internal abstract class Recipient
    {
        protected internal abstract string GetRecipientName();

        public abstract bool CanAccept(Type requestType);

        public abstract bool CanReplyWith(Type requestType, Type responseType);

        protected internal abstract object? Invoke(object request);

        protected internal abstract object? Invoke<TResponse>(object request);

        public async Task<object?> Accept(object request)
        {
            try
            {
                var invocationResult = Invoke(request);
                var completedResult = await UnwrapAsyncResult(invocationResult).ConfigureAwait(false);
                return completedResult;
            }
            catch (TargetInvocationException tIEx) when (tIEx.InnerException is not null)
            {
                ExceptionDispatchInfo.Capture(tIEx.InnerException).Throw();
                return default; // Unreachable
            }
        }

        public async Task<TResponse> ReplyWith<TResponse>(object request)
        {
            try
            {
                var invocationResult = Invoke<TResponse>(request);
                var completedResult = await UnwrapAsyncResult(invocationResult).ConfigureAwait(false);
                return (TResponse)completedResult!; // The response type will match TResponse, even on structs.
            }
            catch (TargetInvocationException tIEx) when (tIEx.InnerException is not null)
            {
                ExceptionDispatchInfo.Capture(tIEx.InnerException).Throw();
                return default!; // Unreachable
            }
        }

        private async Task<object?> UnwrapAsyncResult(object? response)
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

            var description = Awaitable.Describe(response);

            if (description is null)
                throw new Exception("Couldn't extract async response.");

            var awaiter = description.GetAwaiterMethod.Invoke(response, null);
            var awaitedResult = description.AwaiterDescriptor.GetResultMethod.Invoke(awaiter, null);
            return awaitedResult;
        }
    }
}
