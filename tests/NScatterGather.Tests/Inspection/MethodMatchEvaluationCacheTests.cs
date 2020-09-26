using System;
using Xunit;

namespace NScatterGather.Inspection
{
    public class MethodMatchEvaluationCacheTests
    {
        class SomeRequest { }

        class SomeResponse { }

        [Fact]
        public void Error_if_request_type_is_null()
        {
            var cache = new MethodMatchEvaluationCache();
            Assert.Throws<ArgumentNullException>(() => cache.TryAdd(null!, new MethodMatchEvaluation(false, null)));
        }

        [Fact]
        public void Error_if_response_type_is_null()
        {
            var cache = new MethodMatchEvaluationCache();
            Assert.Throws<ArgumentNullException>(() => cache.TryAdd(typeof(SomeRequest), null!, new MethodMatchEvaluation(false, null)));
        }

        [Fact]
        public void Error_if_inspection_is_null()
        {
            var cache = new MethodMatchEvaluationCache();
            Assert.Throws<ArgumentNullException>(() => cache.TryAdd<SomeRequest>(null!));
        }

        [Fact]
        public void Error_if_request_type_is_null_when_searching()
        {
            var cache = new MethodMatchEvaluationCache();
            Assert.Throws<ArgumentNullException>(() => cache.TryFindEvaluation(null!, out _));
            Assert.Throws<ArgumentNullException>(() => cache.TryFindEvaluation(null!, typeof(SomeResponse), out _));
        }

        [Fact]
        public void Error_if_response_type_is_null_when_searching()
        {
            var cache = new MethodMatchEvaluationCache();
            Assert.Throws<ArgumentNullException>(() => cache.TryFindEvaluation(typeof(SomeRequest), null!, out _));
        }

        [Fact]
        public void Accepts_generic_request_type()
        {
            var cache = new MethodMatchEvaluationCache();
            bool added = cache.TryAdd<SomeRequest>(new MethodMatchEvaluation(false, null));
            Assert.True(added);
        }

        [Fact]
        public void Accepts_explicit_request_type()
        {
            var cache = new MethodMatchEvaluationCache();
            bool added = cache.TryAdd(typeof(SomeRequest), new MethodMatchEvaluation(false, null));
            Assert.True(added);
        }

        [Fact]
        public void Accepts_generic_request_and_response_types()
        {
            var cache = new MethodMatchEvaluationCache();
            bool added = cache.TryAdd<SomeRequest, SomeResponse>(new MethodMatchEvaluation(false, null));
            Assert.True(added);
        }

        [Fact]
        public void Accepts_explicit_request_and_response_types()
        {
            var cache = new MethodMatchEvaluationCache();

            bool added = cache.TryAdd(
                typeof(SomeRequest),
                typeof(SomeResponse),
                new MethodMatchEvaluation(false, null));

            Assert.True(added);
        }

        [Fact]
        public void Ignores_duplicate_request_type()
        {
            var cache = new MethodMatchEvaluationCache();

            var evaluation = new MethodMatchEvaluation(false, null);
            _ = cache.TryAdd<SomeRequest>(evaluation);

            {
                bool added = cache.TryAdd<SomeRequest>(evaluation);
                Assert.False(added);
            }

            {
                bool added = cache.TryAdd(typeof(SomeRequest), evaluation);
                Assert.False(added);
            }
        }

        [Fact]
        public void Ignores_duplicate_request_and_response_types()
        {
            var cache = new MethodMatchEvaluationCache();

            var evaluation = new MethodMatchEvaluation(false, null);
            _ = cache.TryAdd<SomeRequest, SomeResponse>(evaluation);

            {
                bool added = cache.TryAdd<SomeRequest, SomeResponse>(evaluation);
                Assert.False(added);
            }

            {
                bool added = cache.TryAdd(
                    typeof(SomeRequest),
                    typeof(SomeResponse),
                    evaluation);

                Assert.False(added);
            }
        }

        [Fact]
        public void Finds_generic_request_type()
        {
            var cache = new MethodMatchEvaluationCache();

            var evaluation = new MethodMatchEvaluation(false, null);
            _ = cache.TryAdd<SomeRequest>(evaluation);

            bool found = cache.TryFindEvaluation<SomeRequest>(out var cached);

            Assert.True(found);
            Assert.Same(evaluation, cached);
        }

        [Fact]
        public void Finds_explicit_request_type()
        {
            var cache = new MethodMatchEvaluationCache();

            var evaluation = new MethodMatchEvaluation(false, null);
            _ = cache.TryAdd(typeof(SomeRequest), evaluation);

            bool found = cache.TryFindEvaluation<SomeRequest>(out var cached);

            Assert.True(found);
            Assert.Same(evaluation, cached);
        }

        [Fact]
        public void Finds_generic_request_and_response_types()
        {
            var cache = new MethodMatchEvaluationCache();

            var evaluation = new MethodMatchEvaluation(false, null);
            _ = cache.TryAdd<SomeRequest, SomeResponse>(evaluation);

            bool found = cache.TryFindEvaluation<SomeRequest, SomeResponse>(out var cached);

            Assert.True(found);
            Assert.Same(evaluation, cached);
        }

        [Fact]
        public void Finds_explicit_request_and_response_types()
        {
            var cache = new MethodMatchEvaluationCache();

            var evaluation = new MethodMatchEvaluation(false, null);
            _ = cache.TryAdd(
                typeof(SomeRequest),
                typeof(SomeResponse),
                evaluation);

            bool found = cache.TryFindEvaluation(
                typeof(SomeRequest),
                typeof(SomeResponse),
                out var cached);

            Assert.True(found);
            Assert.Same(evaluation, cached);
        }
    }
}
