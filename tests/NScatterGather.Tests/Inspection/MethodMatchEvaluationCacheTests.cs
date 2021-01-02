using System;
using System.Reflection;
using Xunit;

namespace NScatterGather.Inspection
{
    public class MethodMatchEvaluationCacheTests
    {
        class SomeRequest { }

        class SomeResponse { }

        [Fact]
        public void Accepts_empty_methods()
        {
            var cache = new MethodMatchEvaluationCache();

            cache.TryAdd(new MethodMatchEvaluation(typeof(SomeRequest), typeof(SomeResponse), methods: Array.Empty<MethodInfo>()));
        }

        [Fact]
        public void Error_if_evaluation_is_null()
        {
            var cache = new MethodMatchEvaluationCache();

            Assert.Throws<ArgumentNullException>(() => cache.TryAdd(null!));
        }

        [Fact]
        public void Error_if_request_type_is_null()
        {
            var cache = new MethodMatchEvaluationCache();

            Assert.Throws<ArgumentNullException>(() => cache.TryAdd(
                new MethodMatchEvaluation(requestType: null!, typeof(SomeResponse), Array.Empty<MethodInfo>())));
        }

        [Fact]
        public void Error_if_methods_are_null()
        {
            var cache = new MethodMatchEvaluationCache();

            Assert.Throws<ArgumentNullException>(() => cache.TryAdd(
                new MethodMatchEvaluation(requestType: typeof(SomeRequest), responseType: null, methods: null!)));
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
        public void Accepts_request_type()
        {
            var cache = new MethodMatchEvaluationCache();
            bool added = cache.TryAdd(new MethodMatchEvaluation(typeof(SomeRequest), null, Array.Empty<MethodInfo>()));
            Assert.True(added);
        }

        [Fact]
        public void Accepts_request_and_response_types()
        {
            var cache = new MethodMatchEvaluationCache();

            bool added = cache.TryAdd(new MethodMatchEvaluation(
                typeof(SomeResponse),
                typeof(SomeResponse),
                Array.Empty<MethodInfo>()));

            Assert.True(added);
        }

        [Fact]
        public void Ignores_duplicate_request_type()
        {
            var cache = new MethodMatchEvaluationCache();

            var evaluation = new MethodMatchEvaluation(typeof(SomeRequest), null, Array.Empty<MethodInfo>());
            _ = cache.TryAdd(evaluation);

            bool added = cache.TryAdd(evaluation);
            Assert.False(added);
        }

        [Fact]
        public void Ignores_duplicate_request_and_response_types()
        {
            var cache = new MethodMatchEvaluationCache();

            var evaluation = new MethodMatchEvaluation(typeof(SomeRequest), typeof(SomeResponse), Array.Empty<MethodInfo>());
            _ = cache.TryAdd(evaluation);

            bool added = cache.TryAdd(evaluation);
            Assert.False(added);
        }

        [Fact]
        public void Finds_request_type()
        {
            var cache = new MethodMatchEvaluationCache();

            var evaluation = new MethodMatchEvaluation(typeof(SomeRequest), null, Array.Empty<MethodInfo>());
            _ = cache.TryAdd(evaluation);

            bool found = cache.TryFindEvaluation(typeof(SomeRequest), out var cached);

            Assert.True(found);
            Assert.Same(evaluation, cached);
        }

        [Fact]
        public void Finds_request_and_response_types()
        {
            var cache = new MethodMatchEvaluationCache();

            var evaluation = new MethodMatchEvaluation(typeof(SomeRequest), typeof(SomeResponse), Array.Empty<MethodInfo>());
            _ = cache.TryAdd(evaluation);

            bool found = cache.TryFindEvaluation(
                typeof(SomeRequest),
                typeof(SomeResponse),
                out var cached);

            Assert.True(found);
            Assert.Same(evaluation, cached);
        }
    }
}
