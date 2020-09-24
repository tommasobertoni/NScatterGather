using System;
using System.Threading.Tasks;
using NScatterGather.Inspection;

namespace NScatterGather.Recipients
{
    internal class Recipient
    {
        public Type Type => _type;

        private readonly Type _type;
        private readonly object _instance;

        public Recipient(object instance)
        {
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
            _type = _instance.GetType();
            TypeInspectorRegistry.Register(_type);
        }

        public Recipient(Type type)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));

            try
            {
                _instance = Activator.CreateInstance(_type);
                TypeInspectorRegistry.Register(_type);
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

            var inspector = TypeInspectorRegistry.Of(_type);
            var accepts = inspector?.HasMethodAccepting(requestType) ?? false;
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

            var inspector = TypeInspectorRegistry.Of(_type);
            var repliesWith = inspector?.HasMethodReturning(requestType, responseType) ?? false;

            return repliesWith;
        }

        public async Task<object?> Accept<TRequest>(TRequest request)
        {
            var inspection = TypeInspectorRegistry.Of(_type);

            if (inspection is null || !inspection.TryGetMethodAccepting<TRequest>(out var method))
                throw new InvalidOperationException(
                    $"Type '{_type.Name}' doesn't support accepting requests of type '{typeof(TRequest).Name}'.");

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

        public async Task<TResponse> ReplyWith<TRequest, TResponse>(TRequest request)
        {
            var inspection = TypeInspectorRegistry.Of(_type);

            if (inspection is null || !inspection.TryGetMethodReturning<TRequest, TResponse>(out var method))
                throw new InvalidOperationException(
                    $"Type '{_type.Name}' doesn't support accepting " +
                    $"requests of type '{typeof(TRequest).Name}' and returning '{typeof(TResponse).Name}'.");

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
    }
}
