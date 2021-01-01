using System;
using System.Collections.Generic;
using System.Reflection;

namespace NScatterGather.Inspection
{
    internal class MethodMatchEvaluation
    {
        public Type RequestType { get; }

        public Type? ResponseType { get; }

        public IReadOnlyList<MethodInfo> Methods { get; }

        public MethodMatchEvaluation(
            Type requestType,
            Type? responseType,
            IReadOnlyList<MethodInfo> methods)
        {
            RequestType = requestType;
            ResponseType = responseType;
            Methods = methods;
        }

        public void Deconstruct(
            out Type requestType,
            out Type? responseType,
            out IReadOnlyList<MethodInfo> methods)
        {
            requestType = RequestType;
            responseType = ResponseType;
            methods = Methods;
        }
    }
}
