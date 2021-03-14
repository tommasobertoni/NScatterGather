using System;
using System.Reflection;
using System.Threading.Tasks;
using NScatterGather.Inspection;
using NScatterGather.Recipients;
using NScatterGather.Recipients.Invokers;
using NScatterGather.Recipients.Run;
using Xunit;
using static NScatterGather.CollisionStrategy;

namespace NScatterGather.Run
{
    public class RecipientRunnerTests
    {
        private readonly Recipient _recipient;
        private readonly Recipient _faultingRecipient;
        private readonly Recipient _anotherFaultingRecipient;

        public RecipientRunnerTests()
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
            Assert.False(runner.HasCompletedSuccessfully);
            Assert.Equal(default, runner.Result);
            Assert.False(runner.HasFaulted);
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

            Assert.True(runner.HasCompletedSuccessfully);
            Assert.Equal("42", runner.Result);

            Assert.False(runner.HasFaulted);
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

            Assert.False(runner.HasCompletedSuccessfully);
            Assert.Equal(default, runner.Result);
            Assert.True(runner.HasFaulted);

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

            Assert.False(runner.HasCompletedSuccessfully);
            Assert.Equal(default, runner.Result);

            Assert.True(runner.HasFaulted);

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

            Assert.False(runner.HasCompletedSuccessfully);
            Assert.Equal(default, runner.Result);

            Assert.True(runner.HasFaulted);

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

            Assert.False(runner.HasCompletedSuccessfully);
            Assert.Equal(default, runner.Result);
            Assert.True(runner.HasFaulted);

            Assert.NotEqual(default, runner.StartedAt);
            Assert.NotEqual(default, runner.FinishedAt);
            Assert.True(runner.FinishedAt >= runner.StartedAt);

            Assert.NotNull(runner.Exception);
            Assert.Equal("An invocation failure.", runner.Exception!.Message);
        }

        [Fact]
        public async Task Aggregate_exception_without_inner_is_handled()
        {
            var aggEx = new AggregateException("Empty inner exceptions");
            var runner = new RecipientRunner<int>(_recipient, new PreparedInvocation<int>(() => throw aggEx, false));

            await runner.Start();
            Assert.Same(aggEx, runner.Exception);
        }

        [Fact]
        public async Task Aggregate_exception_with_inner_is_handled()
        {
            var ex1 = new Exception();
            var ex2 = new Exception();
            var aggEx = new AggregateException(ex1, ex2);
            var runner = new RecipientRunner<int>(_recipient, new PreparedInvocation<int>(() => throw aggEx, false));

            await runner.Start();
            Assert.Same(aggEx, runner.Exception);
        }

        [Fact]
        public async Task Aggregate_exception_with_one_inner_is_handled()
        {
            var ex = new Exception();
            var aggEx = new AggregateException(ex);
            var runner = new RecipientRunner<int>(_recipient, new PreparedInvocation<int>(() => throw aggEx, false));

            await runner.Start();
            Assert.Same(ex, runner.Exception);
        }

        [Fact]
        public async Task Target_invocation_exception_without_inner_is_handled()
        {
            var tiEx = new TargetInvocationException("Empty inner exception", null);
            var runner = new RecipientRunner<int>(_recipient, new PreparedInvocation<int>(() => throw tiEx, false));

            await runner.Start();
            Assert.Same(tiEx, runner.Exception);
        }

        [Fact]
        public async Task Target_invocation_exception_with_inner_is_handled()
        {
            var ex = new Exception();
            var tiEx = new TargetInvocationException(ex);
            var runner = new RecipientRunner<int>(_recipient, new PreparedInvocation<int>(() => throw tiEx, false));

            await runner.Start();
            Assert.Same(ex, runner.Exception);
        }
    }
}
