using System;
using System.Threading.Tasks;
using NScatterGather.Inspection;
using NScatterGather.Recipients;
using Xunit;
using static NScatterGather.CollisionStrategy;

namespace NScatterGather.Run
{
    public class RecipientRunTests
    {
        private readonly Recipient _recipient;
        private readonly Recipient _faultingRecipient;
        private readonly Recipient _anotherFaultingRecipient;

        public RecipientRunTests()
        {
            var registry = new TypeInspectorRegistry();
            _recipient = InstanceRecipient.Create(registry, new SomeType(), name: null, IgnoreRecipient);
            _faultingRecipient = InstanceRecipient.Create(registry, new SomeFaultingType(), name: null, IgnoreRecipient);
            _anotherFaultingRecipient = InstanceRecipient.Create(registry, new SomeComplexFaultingType(), name: null, IgnoreRecipient);
        }

        [Fact]
        public void Can_be_created()
        {
            var runners = _recipient.Accept(42);
            var runner = runners[0];
            Assert.Same(_recipient, runner.Recipient);
        }

        [Fact]
        public void Initially_has_default_parameters()
        {
            var runners = _recipient.Accept(42);
            var runner = runners[0];
            Assert.False(runner.CompletedSuccessfully);
            Assert.Equal(default, runner.Result);
            Assert.False(runner.Faulted);
            Assert.Null(runner.Exception);
            Assert.Equal(default, runner.StartedAt);
            Assert.Equal(default, runner.FinishedAt);
        }

        [Fact]
        public async Task Error_if_started_multiple_times()
        {
            var runners = _recipient.Accept(42);
            var runner = runners[0];
            await runner.Start();
            await Assert.ThrowsAsync<InvalidOperationException>(() => runner.Start());
        }

        [Fact]
        public async Task Runner_completes()
        {
            var runners = _recipient.Accept(42);
            var runner = runners[0];
            await runner.Start();

            Assert.True(runner.CompletedSuccessfully);
            Assert.Equal("42", runner.Result);

            Assert.False(runner.Faulted);
            Assert.Null(runner.Exception);

            Assert.NotEqual(default, runner.StartedAt);
            Assert.NotEqual(default, runner.FinishedAt);
            Assert.True(runner.FinishedAt >= runner.StartedAt);
        }

        [Fact]
        public async Task Runner_fails()
        {
            var runners = _faultingRecipient.Accept(42);
            var runner = runners[0];
            await runner.Start();

            Assert.False(runner.CompletedSuccessfully);
            Assert.Equal(default, runner.Result);
            Assert.True(runner.Faulted);

            Assert.NotEqual(default, runner.StartedAt);
            Assert.NotEqual(default, runner.FinishedAt);
            Assert.True(runner.FinishedAt >= runner.StartedAt);

            Assert.NotNull(runner.Exception);
        }

        [Fact]
        public async Task Exception_is_extracted()
        {
            var runners = _faultingRecipient.Accept(42);
            var runner = runners[0];
            await runner.Start();

            Assert.False(runner.CompletedSuccessfully);
            Assert.Equal(default, runner.Result);

            Assert.True(runner.Faulted);

            Assert.NotEqual(default, runner.StartedAt);
            Assert.NotEqual(default, runner.FinishedAt);
            Assert.True(runner.FinishedAt >= runner.StartedAt);

            Assert.NotNull(runner.Exception);
            Assert.Equal("A failure.", runner.Exception!.Message);
        }

        [Fact]
        public async Task Aggregated_exceptions_are_decomposed()
        {
            var runners = _anotherFaultingRecipient.Accept(42);
            var runner = runners[0];
            await runner.Start();

            Assert.False(runner.CompletedSuccessfully);
            Assert.Equal(default, runner.Result);

            Assert.True(runner.Faulted);

            Assert.NotEqual(default, runner.StartedAt);
            Assert.NotEqual(default, runner.FinishedAt);
            Assert.True(runner.FinishedAt >= runner.StartedAt);

            Assert.NotNull(runner.Exception);
            Assert.IsType<AggregateException>(runner.Exception);

            var aggEx = (AggregateException)runner.Exception!;

            Assert.Equal(3, aggEx.InnerExceptions.Count);

            foreach (var exception in aggEx.InnerExceptions)
            {
                Assert.NotNull(exception);
                Assert.Equal("A failure.", exception!.Message);
            }
        }

        [Fact]
        public async Task Reflection_exception_are_decomposed()
        {
            var runners = _anotherFaultingRecipient.Accept(42L);
            var runner = runners[0];
            await runner.Start();

            Assert.False(runner.CompletedSuccessfully);
            Assert.Equal(default, runner.Result);
            Assert.True(runner.Faulted);

            Assert.NotEqual(default, runner.StartedAt);
            Assert.NotEqual(default, runner.FinishedAt);
            Assert.True(runner.FinishedAt >= runner.StartedAt);

            Assert.NotNull(runner.Exception);
            Assert.Equal("An invocation failure.", runner.Exception!.Message);
        }
    }
}
