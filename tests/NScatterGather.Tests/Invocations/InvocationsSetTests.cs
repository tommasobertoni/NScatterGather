using System;
using System.Threading.Tasks;
using NScatterGather.Recipients;
using Xunit;

namespace NScatterGather.Invocations
{
    public class InvocationsSetTests
    {
        private readonly Invocation<int> _invocation;
        private readonly Invocation<int> _invocationFaulted;
        private readonly Invocation<int> _invocationIncomplete;

        public InvocationsSetTests()
        {
            var recipient = new Recipient(typeof(object));
            _invocation = new Invocation<int>(recipient, Task.FromResult(42));
            _invocationFaulted = new Invocation<int>(recipient, Task.FromException<int>(new Exception()));
            _invocationIncomplete = new Invocation<int>(recipient, GetInfiniteTask<int>());

            // Local functions.

            static Task<TResult> GetInfiniteTask<TResult>()
            {
                var source = new TaskCompletionSource<TResult>();
                return source.Task;
            }
        }

        [Fact]
        public void Invocations_can_be_added()
        {
            var set = new InvocationsSet<int>();
            set.Add(_invocation);
            set.Add(_invocationFaulted);
            set.Add(_invocationIncomplete);
            Assert.Equal(3, set.TotalInvocationsCount);
            Assert.Single(set.Completed);
            Assert.Single(set.Faulted);
            Assert.Single(set.Incomplete);
        }

        [Fact]
        public void Invocations_can_be_passed_to_the_constructor()
        {
            var invocations = new[] { _invocation, _invocationFaulted, _invocationIncomplete };
            var set = new InvocationsSet<int>(invocations);
            Assert.Equal(3, set.TotalInvocationsCount);
            Assert.Single(set.Completed);
            Assert.Single(set.Faulted);
            Assert.Single(set.Incomplete);
        }

        [Fact]
        public void Can_be_deconstructed()
        {
            var invocations = new[] { _invocation, _invocationFaulted, _invocationIncomplete };
            var set = new InvocationsSet<int>(invocations);

            var (completed, faulted, incomplete) = set;
            Assert.Single(completed, _invocation);
            Assert.Single(faulted, _invocationFaulted);
            Assert.Single(incomplete, _invocationIncomplete);
        }
    }
}
