using Xunit;

namespace NScatterGather.Inspection
{
    public class TypeInspectorCacheTests
    {
        class SomeRequest { }

        class SomeResponse { }

        [Fact]
        public void Accepts_generic_request_type()
        {
            var cache = new TypeInspectionCache();
            bool added = cache.TryAdd<SomeRequest>(new TypeInspection(false, null));
            Assert.True(added);
        }

        [Fact]
        public void Accepts_explicit_request_type()
        {
            var cache = new TypeInspectionCache();
            bool added = cache.TryAdd(typeof(SomeRequest), new TypeInspection(false, null));
            Assert.True(added);
        }

        [Fact]
        public void Accepts_generic_request_and_response_types()
        {
            var cache = new TypeInspectionCache();
            bool added = cache.TryAdd<SomeRequest, SomeResponse>(new TypeInspection(false, null));
            Assert.True(added);
        }

        [Fact]
        public void Accepts_explicit_request_and_response_types()
        {
            var cache = new TypeInspectionCache();

            bool added = cache.TryAdd(
                typeof(SomeRequest),
                typeof(SomeResponse),
                new TypeInspection(false, null));

            Assert.True(added);
        }

        [Fact]
        public void Ignores_duplicate_request_type()
        {
            var cache = new TypeInspectionCache();

            var inspection = new TypeInspection(false, null);
            _ = cache.TryAdd<SomeRequest>(inspection);

            {
                bool added = cache.TryAdd<SomeRequest>(inspection);
                Assert.False(added);
            }

            {
                bool added = cache.TryAdd(typeof(SomeRequest), inspection);
                Assert.False(added);
            }
        }

        [Fact]
        public void Ignores_duplicate_request_and_response_types()
        {
            var cache = new TypeInspectionCache();

            var inspection = new TypeInspection(false, null);
            _ = cache.TryAdd<SomeRequest, SomeResponse>(inspection);

            {
                bool added = cache.TryAdd<SomeRequest, SomeResponse>(inspection);
                Assert.False(added);
            }

            {
                bool added = cache.TryAdd(
                    typeof(SomeRequest),
                    typeof(SomeResponse),
                    inspection);

                Assert.False(added);
            }
        }

        [Fact]
        public void Finds_generic_request_type()
        {
            var cache = new TypeInspectionCache();

            var inspection = new TypeInspection(false, null);
            _ = cache.TryAdd<SomeRequest>(inspection);

            bool found = cache.TryFindInspectionResult<SomeRequest>(out var cached);

            Assert.True(found);
            Assert.Same(inspection, cached);
        }

        [Fact]
        public void Finds_explicit_request_type()
        {
            var cache = new TypeInspectionCache();

            var inspection = new TypeInspection(false, null);
            _ = cache.TryAdd(typeof(SomeRequest), inspection);

            bool found = cache.TryFindInspectionResult<SomeRequest>(out var cached);

            Assert.True(found);
            Assert.Same(inspection, cached);
        }

        [Fact]
        public void Finds_generic_request_and_response_types()
        {
            var cache = new TypeInspectionCache();

            var inspection = new TypeInspection(false, null);
            _ = cache.TryAdd<SomeRequest, SomeResponse>(inspection);

            bool found = cache.TryFindInspectionResult<SomeRequest, SomeResponse>(out var cached);

            Assert.True(found);
            Assert.Same(inspection, cached);
        }

        [Fact]
        public void Finds_explicit_request_and_response_types()
        {
            var cache = new TypeInspectionCache();

            var inspection = new TypeInspection(false, null);
            _ = cache.TryAdd(
                typeof(SomeRequest),
                typeof(SomeResponse),
                inspection);

            bool found = cache.TryFindInspectionResult(
                typeof(SomeRequest),
                typeof(SomeResponse),
                out var cached);

            Assert.True(found);
            Assert.Same(inspection, cached);
        }
    }
}
