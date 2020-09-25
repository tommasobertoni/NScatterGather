using System.Threading.Tasks;
using Xunit;

namespace NScatterGather.Inspection
{
    public class MethodAnalyzerTests
    {
        class SomeType
        {
            public void DoVoid() { }

            public void AcceptIntVoid(int n) { }

            public string EchoString(string s) => s;

            public Task DoTask() => Task.CompletedTask;

            public Task<int> ReturnTask(int n) => Task.FromResult(n);

            public string Multi(int n, string s) => "Don't Panic";
        }

        public enum Check
        {
            Request,
            RequestAndResponse
        }

        private readonly MethodInspection _doVoidInspection;
        private readonly MethodInspection _acceptIntVoidInspection;
        private readonly MethodInspection _echoStringInspection;
        private readonly MethodInspection _doTaskInspection;
        private readonly MethodInspection _returnTaskInspection;
        private readonly MethodInspection _multiInspection;

        public MethodAnalyzerTests()
        {
            var t = typeof(SomeType);

            _doVoidInspection = new MethodInspection(
                t, t.GetMethod(nameof(SomeType.DoVoid))!);

            _acceptIntVoidInspection = new MethodInspection(
                t, t.GetMethod(nameof(SomeType.AcceptIntVoid))!);

            _echoStringInspection = new MethodInspection(
                t, t.GetMethod(nameof(SomeType.EchoString))!);

            _doTaskInspection = new MethodInspection(
                t, t.GetMethod(nameof(SomeType.DoTask))!);

            _returnTaskInspection = new MethodInspection(
                t, t.GetMethod(nameof(SomeType.ReturnTask))!);

            _multiInspection = new MethodInspection(
                t, t.GetMethod(nameof(SomeType.Multi))!);
        }

        [Fact]
        public void Method_without_parameters_matches()
        {
            var analyzer = new MethodAnalyzer();

            bool isMatch = analyzer.IsMatch(_doVoidInspection, typeof(void), out var match);

            Assert.True(isMatch);
            Assert.NotNull(match);
            Assert.Equal(typeof(SomeType).GetMethod(nameof(SomeType.DoVoid)), match);
        }

        [Fact]
        public void Method_returning_void_matches()
        {
            var analyzer = new MethodAnalyzer();

            bool isMatch = analyzer.IsMatch(_doVoidInspection, typeof(void), typeof(void), out var match);

            Assert.True(isMatch);
            Assert.NotNull(match);
            Assert.Equal(typeof(SomeType).GetMethod(nameof(SomeType.DoVoid)), match);
        }

        [Theory]
        [InlineData(Check.Request)]
        [InlineData(Check.RequestAndResponse)]
        public void Method_with_parameters_returning_void_matches(Check check)
        {
            var analyzer = new MethodAnalyzer();

            bool isMatch = check == Check.RequestAndResponse
                ? analyzer.IsMatch(_acceptIntVoidInspection, typeof(int), typeof(void), out var match)
                : analyzer.IsMatch(_acceptIntVoidInspection, typeof(int), out match);

            Assert.True(isMatch);
            Assert.NotNull(match);
            Assert.Equal(typeof(SomeType).GetMethod(nameof(SomeType.AcceptIntVoid)), match);
        }

        [Theory]
        [InlineData(Check.Request)]
        [InlineData(Check.RequestAndResponse)]
        public void Method_with_parameters_returning_result_matches(Check check)
        {
            var analyzer = new MethodAnalyzer();

            bool isMatch = check == Check.RequestAndResponse
                ? analyzer.IsMatch(_echoStringInspection, typeof(string), typeof(string), out var match)
                : analyzer.IsMatch(_echoStringInspection, typeof(string), out match);

            Assert.True(isMatch);
            Assert.NotNull(match);
            Assert.Equal(typeof(SomeType).GetMethod(nameof(SomeType.EchoString)), match);
        }

        [Theory]
        [InlineData(Check.Request)]
        [InlineData(Check.RequestAndResponse)]
        public void Method_returning_task_matches(Check check)
        {
            var analyzer = new MethodAnalyzer();

            bool isMatch = check == Check.RequestAndResponse
                ? analyzer.IsMatch(_doTaskInspection, typeof(void), typeof(Task), out var match)
                : analyzer.IsMatch(_doTaskInspection, typeof(void), out match);

            Assert.True(isMatch);
            Assert.NotNull(match);
            Assert.Equal(typeof(SomeType).GetMethod(nameof(SomeType.DoTask)), match);
        }

        [Fact]
        public void Method_returning_task_does_not_return_void()
        {
            var analyzer = new MethodAnalyzer();
            bool isMatch = analyzer.IsMatch(_doTaskInspection, typeof(void), typeof(void), out var match);
            Assert.False(isMatch);
            Assert.Null(match);
        }

        [Theory]
        [InlineData(Check.Request)]
        [InlineData(Check.RequestAndResponse)]
        public void Method_returning_generic_task_matches(Check check)
        {
            var analyzer = new MethodAnalyzer();

            bool isMatch = check == Check.RequestAndResponse
                ? analyzer.IsMatch(_returnTaskInspection, typeof(int), typeof(Task<int>), out var match)
                : analyzer.IsMatch(_returnTaskInspection, typeof(int), out match);

            Assert.True(isMatch);
            Assert.NotNull(match);
            Assert.Equal(typeof(SomeType).GetMethod(nameof(SomeType.ReturnTask)), match);
        }

        [Fact]
        public void Generic_task_does_not_equal_task()
        {
            var analyzer = new MethodAnalyzer();

            bool isMatch = analyzer.IsMatch(_returnTaskInspection, typeof(int), typeof(Task), out var match);

            Assert.False(isMatch);
            Assert.Null(match);
        }

        [Theory]
        [InlineData(Check.Request)]
        [InlineData(Check.RequestAndResponse)]
        public void Method_with_multiple_parameters_does_not_match(Check check)
        {
            var analyzer = new MethodAnalyzer();

            bool isMatch = check == Check.RequestAndResponse
                ? analyzer.IsMatch(_multiInspection, typeof(int), typeof(string), out var match)
                : analyzer.IsMatch(_multiInspection, typeof(int), out match);

            Assert.False(isMatch);
            Assert.Null(match);
        }

        [Theory]
        [InlineData(Check.Request)]
        [InlineData(Check.RequestAndResponse)]
        public void Method_with_wrong_parameter_does_not_match(Check check)
        {
            var analyzer = new MethodAnalyzer();

            bool isMatch = check == Check.RequestAndResponse
                ? analyzer.IsMatch(_echoStringInspection, typeof(int), typeof(string), out var match)
                : analyzer.IsMatch(_echoStringInspection, typeof(int), out match);

            Assert.False(isMatch);
            Assert.Null(match);
        }
    }
}
