using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NScatterGather.Samples;
using NScatterGather.Samples.Samples;
using Spectre.Console;
using static System.Console;

await Run<HelloWorld>();
await Run<FilterOnResponse>();
await Run<InvokeAsyncMethods>();
await Run<HandleErrors>();
await Run<Timeout>();

// Beautify CLI Samples!

static async Task Run<TSample>() where TSample : ISample, new()
{
    var sampleName = typeof(TSample).Name;

    // PascalCase to Title Case
    var title = Regex.Replace(sampleName, "(\\B[A-Z])", " $1");

    var rule = new Rule(title)
        .RuleStyle("steelblue1")
        .LeftAligned();

    AnsiConsole.Render(rule);

    await new TSample().Run();

    AnsiConsole.WriteLine();
}
