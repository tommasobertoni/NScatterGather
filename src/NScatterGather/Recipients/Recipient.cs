using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using IsAwaitable.Analysis;
using NScatterGather.Inspection;

namespace NScatterGather.Recipients
{
    internal class Recipient
    {
        public Type Type => _type;

        private readonly Type _type;
        private readonly object _instance;
        private readonly TypeInspector _inspector;

        public Recipient(
            object instance,
            TypeInspector? inspector = null)
        {
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
            _type = _instance.GetType();
            _inspector = inspector ?? new TypeInspector(_type);
        }

        public Recipient(
            Type type,
            TypeInspector? inspector = null)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _inspector = inspector ?? new TypeInspector(_type);

            try
            {
                _instance = Activator.CreateInstance(_type)!;
            }
            catch (MissingMethodException mMEx)
            {
                throw new InvalidOperationException($"Could not create a new instance of type '{_type.Name}'.", mMEx);
            }
        }

        public bool CanAccept<TRequest>() =>
            CanAccept(typeof(TRequest));

        public bool CanAccept(Type requestType)
        {
            if (requestType is null)
                throw new ArgumentNullException(nameof(requestType));

            var accepts = _inspector.HasMethodAccepting(requestType);
            return accepts;
        }

        public bool CanReplyWith<TRequest, TResponse>() =>
            CanReplyWith(typeof(TRequest), typeof(TResponse));

        public bool CanReplyWith(Type requestType, Type responseType)
        {
            if (requestType is null)
                throw new ArgumentNullException(nameof(requestType));

            if (responseType is null)
                throw new ArgumentNullException(nameof(responseType));

            var repliesWith = _inspector.HasMethodReturning(requestType, responseType);
            return repliesWith;
        }

        public async Task<object?> Accept<TRequest>(TRequest request)
        {
            if (!_inspector.TryGetMethodAccepting<TRequest>(out var method))
                throw new InvalidOperationException(
                    $"Type '{_type.Name}' doesn't support accepting requests of type '{typeof(TRequest).Name}'.");

            try
            {
                var response = await Invoke(method, request).ConfigureAwait(false);
                return response;
            }
            catch (TargetInvocationException tIEx) when (tIEx.InnerException is not null)
            {
                ExceptionDispatchInfo.Capture(tIEx.InnerException).Throw();
                return default; // Unreachable
            }
        }

        public async Task<TResponse> ReplyWith<TRequest, TResponse>(TRequest request)
        {
            if (!_inspector.TryGetMethodReturning<TRequest, TResponse>(out var method))
                throw new InvalidOperationException(
                    $"Type '{_type.Name}' doesn't support accepting " +
                    $"requests of type '{typeof(TRequest).Name}' and " +
                    $"returning '{typeof(TResponse).Name}'.");

            try
            {
                var response = await Invoke(method, request);
                return (TResponse)response!; // The response type will match TResponse, even on structs.
            }
            catch (TargetInvocationException tIEx) when (tIEx.InnerException is not null)
            {
                ExceptionDispatchInfo.Capture(tIEx.InnerException).Throw();
                return default!; // Unreachable
            }
        }

        private async Task<object?> Invoke(MethodInfo method, object? request)
        {
            var response = method.Invoke(_instance, new object[] { request! });

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
