using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NScatterGather.Inspection
{
    internal class MethodAnalyzer
    {
        public bool IsMatch(
            MethodInspection inspection,
            Type requestType,
            [NotNullWhen(true)] out MethodInfo? match)
        {
            match = null;

            var (_, method, parameters, _) = inspection;

            if (parameters.Count == 0 && requestType == typeof(void))
            {
                match = method;
                return true;
            }

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
            [NotNullWhen(true)] out MethodInfo? match)
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

            // The return type may be an awaitable!
            // (Task<TResponse>, ValueTask<TResponse>, ...)

            if (!returnType.IsAwaitableWithResult(out var awaitResultType))
                return false;

            if (awaitResultType != responseType)
                return false;

            // It's a match: Task/ValueTask/Awaitable of TResponse.

            match = method;
            return true;
        }
    }
}
