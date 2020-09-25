using Xunit;

namespace NScatterGather.Inspection
{
    public class TypeInspectionTests
    {
        [Fact]
        public void Can_be_constructed()
        {
            new TypeInspection(false, typeof(object).GetMethod(nameof(object.ToString)));
        }

        [Fact]
        public void Can_be_deconstructed()
        {
            var inspection = new TypeInspection(
                false,
                typeof(object).GetMethod(nameof(object.ToString)));

            var (isMatch, method) = inspection;
            Assert.Equal(isMatch, inspection.IsMatch);
            Assert.Equal(method, inspection.Method);
        }
    }
}
