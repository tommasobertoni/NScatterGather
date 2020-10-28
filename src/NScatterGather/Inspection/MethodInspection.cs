using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace NScatterGather.Inspection
{
    internal class MethodInspection
    {
        public Type InspectedType { get; }

        public MethodInfo InspectedMethod { get; }

        public IReadOnlyList<ParameterInfo> Parameters { get; }

        public Type ReturnType { get; }

        public bool ReturnsAResponse { get; }

        internal MethodInspection(
            Type inspectedType,
            MethodInfo method)
        {
            InspectedType = inspectedType ??
                throw new ArgumentNullException(nameof(inspectedType));

            InspectedMethod = method ??
                throw new ArgumentNullException(nameof(method));

            Parameters = method.GetParameters();
            ReturnType = method.ReturnType;

            ReturnsAResponse = ReturnType.IsAwaitable()
                ? ReturnType.IsAwaitableWithResult()
                : ReturnType != typeof(void);
        }

        internal void Deconstruct(
            out Type inspectedType,
            out MethodInfo inspectedMethod,
            out IReadOnlyList<ParameterInfo> parameters,
            out Type returnType)
        {
            inspectedType = InspectedType;
            inspectedMethod = InspectedMethod;
            parameters = Parameters;
            returnType = ReturnType;
        }
    }
}
