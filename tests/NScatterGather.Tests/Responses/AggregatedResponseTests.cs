﻿using NScatterGather.Inspection;
using NScatterGather.Recipients;
using NScatterGather.Recipients.Run;
using Xunit;

namespace NScatterGather.Responses
{
    public class AggregatedResponseTests
    {
        private readonly RecipientRun<object?>[] _runners;

        public AggregatedResponseTests()
        {
            var registry = new TypeInspectorRegistry();

            var someRecipient = InstanceRecipient.Create(registry, new SomeType(), name: null);
            var someRun = someRecipient.Accept(42);
            someRun.Start().Wait();

            var someFaultingRecipient = InstanceRecipient.Create(registry, new SomeFaultingType(), name: null);
            var someFaultingRun = someFaultingRecipient.Accept(42);
            someFaultingRun.Start().Wait();

            var someNeverEndingRecipient = InstanceRecipient.Create(registry, new SomeNeverEndingType(), name: null);
            var someNeverEndingRun = someNeverEndingRecipient.Accept(42);
            someNeverEndingRun.Start();

            _runners = new[] { someRun, someFaultingRun, someNeverEndingRun };
        }

        [Fact]
        public void Can_be_created()
        {
            var response = AggregatedResponseFactory.CreateFrom(_runners);
            Assert.Equal(_runners.Length, response.TotalInvocationsCount);
            Assert.Single(response.Completed);
            Assert.Single(response.Faulted);
            Assert.Single(response.Incomplete);
        }

        [Fact]
        public void Invocations_are_grouped_correctly()
        {
            var response = AggregatedResponseFactory.CreateFrom(_runners);

            Assert.Equal(typeof(SomeType), response.Completed[0].RecipientType);
            Assert.Equal("42", response.Completed[0].Result);

            Assert.Equal(typeof(SomeFaultingType), response.Faulted[0].RecipientType);
            Assert.Equal("A failure.", response.Faulted[0].Exception?.Message);

            Assert.Equal(typeof(SomeNeverEndingType), response.Incomplete[0].RecipientType);
        }

        [Fact]
        public void Can_be_deconstructed()
        {
            var response = AggregatedResponseFactory.CreateFrom(_runners);
            var (completed, faulted, incomplete) = response;
            Assert.Single(completed);
            Assert.Single(faulted);
            Assert.Single(incomplete);
        }
    }
}
