using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace NScatterGather.Inspection
{
    internal class MethodAnalyzer
    {
        public bool IsMatch(
            MethodInspection inspection,
            Type requestType,
            bool allowCancellationTokenParameter,
            [NotNullWhen(true)] out MethodInfo? match)
        {
            match = null;

            var (_, method, parameters, _) = inspection;

            if (parameters.Count == 0 && requestType == typeof(void))
            {
                match = method;
                return true;
            }

            if (parameters.Count > 2)
                return false;

            if (parameters.Count == 2)
            {
                if (!allowCancellationTokenParameter)
                    return false;

                if (!AcceptsCancellationToken(inspection))
                    return false;
            }

            var theParameter = parameters[0];

            if (IsSameOrCompatible(baseType: theParameter.ParameterType, requestType))
            {
                match = method;
                return true;
            }

            return false;
        }

        public bool IsMatch(
            MethodInspection inspection,
            Type requestType,
            Type responseType,
            bool allowCancellationTokenParameter,
            [NotNullWhen(true)] out MethodInfo? match)
        {
            match = null;

            if (!IsMatch(inspection, requestType, allowCancellationTokenParameter, out _))
                return false;

            // Method has the correct input parameter.
            // Check the return type!

            var (_, method, _, returnType) = inspection;

            if (IsSameOrCompatible(baseType: responseType, returnType))
            {
                match = method;
                return true;
            }

            // The return type may be an awaitable!
            // (Task<TResponse>, ValueTask<TResponse>, ...)

            if (!returnType.IsAwaitableWithResult(out var awaitResultType))
                return false;

            if (IsSameOrCompatible(baseType: responseType, awaitResultType))
            {
                // It's a match: Task/ValueTask/Awaitable of TResponse.
                match = method;
                return true;
            }

            return false;
        }

        public bool AcceptsCancellationToken(MethodInspection inspection) =>
            AcceptCancellationToken(inspection.Parameters);

        public bool AcceptsCancellationToken(MethodInfo method) =>
            AcceptCancellationToken(method.GetParameters());

        private bool AcceptCancellationToken(IReadOnlyList<ParameterInfo> parameters)
        {
            if (parameters.Count != 2)
                return false;

            var theCancellationTokenParameter = parameters[1];

            return theCancellationTokenParameter.ParameterType == typeof(CancellationToken);
        }

        private bool IsSameOrCompatible(Type baseType, Type otherType)
        {
            if (baseType == otherType)
                return true;

            var nonNullableBaseType = Nullable.GetUnderlyingType(baseType);

            if (nonNullableBaseType is not null &&
                nonNullableBaseType == otherType)
                return true;

            return false;
        }
    }
}
