using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NScatterGather.Inspection
{
    internal class MethodAnalyzer
    {
        private static readonly Type _taskType;
        private static readonly Type _valueTaskType;

        static MethodAnalyzer()
        {
            _taskType = typeof(Task);
            _valueTaskType = typeof(ValueTask);
        }

        public bool IsMatch(
            MethodInspection inspection,
            Type requestType,
            [MaybeNullWhen(false)] out MethodInfo match)
        {
            match = null;

            var (_, method, parameters, _) = inspection;

            if (parameters.Count != 1)
                return false;

            var theParameter = parameters.First();

            if (theParameter.ParameterType != requestType)
                return false;

            match = method;
            return true;
        }

        public bool IsMatch(
            MethodInspection inspection,
            Type requestType,
            Type responseType,
            [MaybeNullWhen(false)] out MethodInfo match)
        {
            match = null;

            if (!IsMatch(inspection, requestType, out _))
                return false;

            // Method has the correct input parameter.
            // Check the return type!

            var (_, method, _, returnType) = inspection;

            if (returnType == responseType)
            {
                match = method;
                return true;
            }

            // The return type may be Task<TResponse>!

            var isReturningTask = _taskType.IsAssignableFrom(returnType);
            var isReturningValueTask = _valueTaskType.IsAssignableFrom(returnType);

            if (!(isReturningTask || isReturningValueTask))
                return false;

            // Since it's returning an awaitable (Task/ValueTask)
            // let's check the type of the promise.

            if (!returnType.IsGenericType)
                return false;

            var genericArguments = returnType.GetGenericArguments();

            if (genericArguments.Length != 1)
                return false;

            var theGenericArgument = genericArguments.First();

            if (theGenericArgument != responseType)
                return false;

            // It's a match: Task/ValueTask of TResponse.

            match = method;
            return true;
        }
    }
}
