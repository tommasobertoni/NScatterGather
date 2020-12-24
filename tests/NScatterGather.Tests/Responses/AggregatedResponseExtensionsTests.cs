using System;
using System.Linq;
using NScatterGather.Inspection;
using NScatterGather.Recipients;
using NScatterGather.Recipients.Run;
using Xunit;

namespace NScatterGather.Responses
{
    public class AggregatedResponseExtensionsTests
    {
        private readonly RecipientRun<object?>[] _runners;

        public AggregatedResponseExtensionsTests()
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
        public void Error_if_input_is_null()
        {
            Assert.Throws<ArgumentNullException>(() =>
                (null as AggregatedResponse<int>)!.AsResultsDictionary());

            Assert.Throws<ArgumentNullException>(() =>
                (null as AggregatedResponse<int>)!.AsResultsList());
        }

        [Fact]
        public void Can_be_projected_onto_results_dictionary()
        {
            var response = AggregatedResponseFactory.CreateFrom(_runners);
            var results = response.AsResultsDictionary();
            Assert.NotNull(results);
            Assert.Single(results.Keys);
            Assert.Equal(typeof(SomeType), results.Keys.First());
            Assert.Single(results.Values);
            Assert.Equal("42", results.Values.First());
        }

        [Fact]
        public void Can_be_projected_onto_results_list()
        {
            var response = AggregatedResponseFactory.CreateFrom(_runners);
            var results = response.AsResultsList();
            Assert.NotNull(results);
            Assert.Single(results, "42");
        }
    }
}
