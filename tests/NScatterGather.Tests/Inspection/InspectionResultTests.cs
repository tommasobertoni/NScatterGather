using System;
using System.Reflection;
using Xunit;

namespace NScatterGather.Inspection
{
    public class InspectionResultTests
    {
        [Fact]
        public void Can_be_constructed()
        {
            _ = new MethodMatchEvaluation(typeof(object), typeof(object), Array.Empty<MethodInfo>());
        }

        [Fact]
        public void Can_be_deconstructed()
        {
            var expectedRequestType = typeof(int);
            var expectedResponseType = typeof(int);
            var expectedMethods = Array.Empty<MethodInfo>();

            var evaluation = new MethodMatchEvaluation(expectedRequestType, expectedResponseType, expectedMethods);
            var (requestType, responseType, methods) = evaluation;

            Assert.Same(expectedRequestType, requestType);
            Assert.Same(expectedResponseType, responseType);
            Assert.Same(expectedMethods, methods);
        }
    }
}
