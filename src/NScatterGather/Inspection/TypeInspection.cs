using System.Reflection;

namespace NScatterGather.Inspection
{
    internal class TypeInspection
    {
        public bool IsMatch { get; }

        public MethodInfo? Method { get; set; }

        public TypeInspection(bool isMatch, MethodInfo? method) =>
            (IsMatch, Method) = (isMatch, method);

        public void Deconstruct(
            out bool isMatch,
            out MethodInfo? method)
        {
            isMatch = IsMatch;
            method = Method;
        }
    }
}
