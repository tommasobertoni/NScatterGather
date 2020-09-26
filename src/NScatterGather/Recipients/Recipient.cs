using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
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
                _instance = Activator.CreateInstance(_type);
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
                var response = method.Invoke(_instance, new object[] { request! });

                if (response is Task taskResponse)
                {
                    await taskResponse;
                    var resultProp = taskResponse.GetType().GetProperty(nameof(Task<object>.Result));
                    var responseValue = resultProp!.GetValue(taskResponse);
                    return responseValue;
                }

                return response;
            }
            catch (TargetInvocationException tIEx)
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
                    $"requests of type '{typeof(TRequest).Name}' and returning '{typeof(TResponse).Name}'.");

            try
            {
                var response = method.Invoke(_instance, new object[] { request! });

                if (response is Task taskResponse)
                {
                    await taskResponse;
                    var resultProp = taskResponse.GetType().GetProperty(nameof(Task<object>.Result));
                    var responseValue = (TResponse)resultProp!.GetValue(taskResponse);
                    return responseValue;
                }

                return (TResponse)response;
            }
            catch (TargetInvocationException tIEx)
            {
                ExceptionDispatchInfo.Capture(tIEx.InnerException).Throw();
                return default; // Unreachable
            }
        }
    }
}
