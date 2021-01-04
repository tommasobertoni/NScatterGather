﻿using System;
using Xunit;

namespace NScatterGather.Invocations
{
    public class FaultedInvocationTests
    {
        [Fact]
        public void Can_be_deconstructed()
        {
            var expectedId = Guid.NewGuid();
            var expectedName = "foo";
            var expectedType = typeof(int);
            var expectedException = new Exception();
            var expectedDuration = TimeSpan.FromSeconds(1);

            var invocation = new FaultedInvocation(
                expectedId,
                expectedName,
                expectedType,
                expectedException,
                expectedDuration);

            var (id, name, type, exception, duration) = invocation;

            Assert.Equal(expectedId, id);
            Assert.Equal(expectedName, name);
            Assert.Equal(expectedType, type);
            Assert.Equal(expectedException, exception);
            Assert.Equal(expectedDuration, duration);
        }
    }
}
