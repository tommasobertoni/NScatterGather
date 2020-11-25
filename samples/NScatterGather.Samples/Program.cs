using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NScatterGather.Samples;
using NScatterGather.Samples.Samples;
using static System.Console;

await Run<HelloWorld>();
await Run<FilterOnResponse>();
await Run<InvokeAsyncMethods>();
await Run<HandleErrors>();
await Run<Timeout>();

#region Beautify CLI Samples!

static async Task Run<TSample>() where TSample : ISample, new()
{
    var title = GetTitleFor<TSample>();
    var header = CreateHeader(title);

    WriteLine(header);
    await new TSample().Run();
    WriteLine();
}

static string GetTitleFor<TSample>() where TSample : ISample, new()
{
    var sampleName = typeof(TSample).Name;
    // PascalCase to Title Case
    var title = Regex.Replace(sampleName, "(\\B[A-Z])", " $1");
    return title;
}

static string CreateHeader(string title)
{
    const int headerLength = 42;
    var separator = '=';

    var sb = new StringBuilder();
    sb.Append(separator);
    sb.Append(separator);
    sb.Append(' ');
    sb.Append(title);
    sb.Append(' ');

    var remainingChars = headerLength - sb.ToString().Length;
    var remainingSeparator = new string(separator, remainingChars);
    sb.Append(remainingSeparator);

    var header = sb.ToString();
    return header;
}

#endregion
