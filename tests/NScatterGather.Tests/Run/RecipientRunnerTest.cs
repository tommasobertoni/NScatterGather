using System;
using System.Threading.Tasks;
using NScatterGather.Recipients;
using Xunit;

namespace NScatterGather.Run
{
    public class RecipientRunnerTest
    {
        private readonly Recipient _recipient;

        public RecipientRunnerTest()
        {
            _recipient = TypeRecipient.Create<object>();
        }

        [Fact]
        public void Can_be_created()
        {
            var runner = new RecipientRunner<int>(_recipient);
            Assert.Same(_recipient, runner.Recipient);
        }

        [Fact]
        public void Initially_has_default_parameters()
        {
            var runner = new RecipientRunner<int>(_recipient);
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
            var runner = new RecipientRunner<int>(_recipient);

            await runner.Run(_ => Task.FromResult(42));

            await Assert.ThrowsAsync<InvalidOperationException>(() => runner.Run(_ => Task.FromResult(42)));
        }

        [Fact]
        public void Error_if_recipient_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new RecipientRunner<int>((null as Recipient)!));
        }

        [Fact]
        public void Error_if_invocation_is_null()
        {
            var runner = new RecipientRunner<int>(_recipient);

            Assert.ThrowsAsync<ArgumentNullException>(() => runner.Run(null!));
        }

        [Fact]
        public async Task Runner_completes()
        {
            var runner = new RecipientRunner<int>(_recipient);

            await runner.Run(_ => Task.FromResult(42));

            Assert.True(runner.CompletedSuccessfully);
            Assert.Equal(42, runner.Result);

            Assert.False(runner.Faulted);
            Assert.Null(runner.Exception);

            Assert.NotEqual(default, runner.StartedAt);
            Assert.NotEqual(default, runner.FinishedAt);
            Assert.True(runner.FinishedAt >= runner.StartedAt);
        }

        [Fact]
        public async Task Runner_fails()
        {
            var runner = new RecipientRunner<int>(_recipient);

            var ex = new Exception();

            await runner.Run(async _ =>
            {
                await Task.Yield();
                throw ex;
            });

            Assert.False(runner.CompletedSuccessfully);
            Assert.Equal(default, runner.Result);
            Assert.True(runner.Faulted);

            Assert.NotEqual(default, runner.StartedAt);
            Assert.NotEqual(default, runner.FinishedAt);
            Assert.True(runner.FinishedAt >= runner.StartedAt);

            Assert.Equal(ex, runner.Exception);
        }

        [Fact]
        public async Task Exception_is_extracted()
        {
            var runner = new RecipientRunner<int>(_recipient);

            var ex = new Exception();

            await runner.Run(_ =>
            {
                Fail().Wait();
                return Task.FromResult(42);
            });

            Assert.False(runner.CompletedSuccessfully);
            Assert.Equal(default, runner.Result);

            Assert.True(runner.Faulted);

            Assert.NotEqual(default, runner.StartedAt);
            Assert.NotEqual(default, runner.FinishedAt);
            Assert.True(runner.FinishedAt >= runner.StartedAt);

            Assert.NotNull(runner.Exception);
            Assert.Equal(ex, runner.Exception);

            // Local functions.

            async Task Fail()
            {
                await Task.Delay(10);
                throw ex;
            }
        }

        [Fact]
        public async Task Aggregated_exceptions_are_decomposed()
        {
            var runner = new RecipientRunner<int>(_recipient);

            var ex = new Exception();

            await runner.Run(_ =>
            {
                Task.WhenAll(Fail(), Fail(), Fail()).Wait();
                return Task.FromResult(42);
            });

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
                Assert.Equal(ex, exception);

            // Local functions.

            async Task Fail()
            {
                await Task.Delay(10);
                throw ex;
            }
        }

        [Fact]
        public async Task Reflection_exception_are_decomposed()
        {
            var runner = new RecipientRunner<int>(_recipient);

            var ex = new Exception();

            await runner.Run(_ =>
            {
                throw new System.Reflection.TargetInvocationException(ex);
            });

            Assert.False(runner.CompletedSuccessfully);
            Assert.Equal(default, runner.Result);
            Assert.True(runner.Faulted);

            Assert.NotEqual(default, runner.StartedAt);
            Assert.NotEqual(default, runner.FinishedAt);
            Assert.True(runner.FinishedAt >= runner.StartedAt);

            Assert.Equal(ex, runner.Exception);
        }
    }
}
