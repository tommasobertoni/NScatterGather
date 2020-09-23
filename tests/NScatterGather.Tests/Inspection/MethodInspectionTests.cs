using System;
using System.Reflection;
using Xunit;

namespace NScatterGather.Inspection
{
    public class MethodInspectionTests
    {
        class SomeType
        {
            public int AMethod(
                Guid guid,
                string s,
                IDisposable d) => 42;
        }

        private readonly Type _inspectedType;
        private readonly MethodInfo _inspectedMethod;

        public MethodInspectionTests()
        {
            _inspectedType = typeof(SomeType);
            _inspectedMethod = _inspectedType.GetMethod(nameof(SomeType.AMethod))!;
        }

        [Fact]
        public void Constructor_parameters_are_used()
        {
            var inspection = new MethodInspection(_inspectedType, _inspectedMethod);

            Assert.NotNull(inspection.InspectedType);
            Assert.Same(_inspectedType, inspection.InspectedType);
            Assert.NotNull(inspection.InspectedMethod);
            Assert.Same(_inspectedMethod, inspection.InspectedMethod);
        }

        [Fact]
        public void Error_if_type_parameter_is_null()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new MethodInspection(null!, _inspectedMethod));
        }

        [Fact]
        public void Error_if_method_parameter_is_null()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new MethodInspection(_inspectedType, null!));
        }

        [Fact]
        public void Parameters_are_correct()
        {
            var inspection = new MethodInspection(_inspectedType, _inspectedMethod);

            Assert.NotNull(inspection.Parameters);
            Assert.NotEmpty(inspection.Parameters);
            Assert.Equal(3, inspection.Parameters.Count);
            Assert.Equal(typeof(Guid), inspection.Parameters[0].ParameterType);
            Assert.Equal(typeof(string), inspection.Parameters[1].ParameterType);
            Assert.Equal(typeof(IDisposable), inspection.Parameters[2].ParameterType);
        }

        [Fact]
        public void Return_type_is_correct()
        {
            var inspection = new MethodInspection(_inspectedType, _inspectedMethod);

            Assert.NotNull(inspection.ReturnType);
            Assert.Equal(typeof(int), inspection.ReturnType);
        }

        [Fact]
        public void Can_be_deconstructed()
        {
            var inspection = new MethodInspection(_inspectedType, _inspectedMethod);

            var (inspectedType, inspectedMethod, parameters, returnType) = inspection;

            Assert.Same(_inspectedType, inspectedType);
            Assert.Same(_inspectedMethod, inspectedMethod);
            Assert.Equal(3, parameters.Count);
            Assert.Equal(typeof(int), returnType);
        }
    }
}
