using System;
using System.Linq;
using NScatterGather.Inspection;
using NScatterGather.Recipients;
using NScatterGather.Recipients.Run;
using Xunit;
using static NScatterGather.CollisionStrategy;

namespace NScatterGather.Responses
{
    public class AggregatedResponseExtensionsTests
    {
        private readonly RecipientRun<object?>[] _runners;

        public AggregatedResponseExtensionsTests()
        {
            var registry = new TypeInspectorRegistry();

            var someRecipient = InstanceRecipient.Create(registry, new SomeType(), name: null, IgnoreRecipient);
            var someRunners = someRecipient.Accept(42);
            var aRunner = someRunners[0];
            aRunner.Start().Wait();

            var someFaultingRecipient = InstanceRecipient.Create(registry, new SomeFaultingType(), name: null, IgnoreRecipient);
            var someFaultingRunners = someFaultingRecipient.Accept(42);
            var aFaultingRunner = someFaultingRunners[0];
            aFaultingRunner.Start().Wait();

            var someNeverEndingRecipient = InstanceRecipient.Create(registry, new SomeNeverEndingType(), name: null, IgnoreRecipient);
            var someNeverEndingRunners = someNeverEndingRecipient.Accept(42);
            var aNeverEndingRunner = someNeverEndingRunners[0];
            aNeverEndingRunner.Start();

            _runners = new[] { aRunner, aFaultingRunner, aNeverEndingRunner };
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
