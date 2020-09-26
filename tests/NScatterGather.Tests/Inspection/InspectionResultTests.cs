using Xunit;

namespace NScatterGather.Inspection
{
    public class InspectionResultTests
    {
        [Fact]
        public void Can_be_constructed()
        {
            new MethodMatchEvaluation(false, typeof(object).GetMethod(nameof(object.ToString)));
        }

        [Fact]
        public void Can_be_deconstructed()
        {
            var evaluation = new MethodMatchEvaluation(
                false,
                typeof(object).GetMethod(nameof(object.ToString)));

            var (isMatch, method) = evaluation;
            Assert.Equal(isMatch, evaluation.IsMatch);
            Assert.Equal(method, evaluation.Method);
        }
    }
}
