using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NScatterGather;
using NScatterGather.Samples.CompetingTasks;
using Spectre.Console;

// Run.

var recipients = CollectRecipients();
var aggregator = new Aggregator(recipients);

var catalog = new Catalog();

var evaluations = await EvaluateCatalog(aggregator, catalog);

PrettyPrint(evaluations);

// Evaluation.

static async Task<IReadOnlyList<Evaluation>> EvaluateCatalog(Aggregator aggregator, Catalog catalog)
{
    var evaluations = new List<Evaluation>();

    await AnsiConsole.Status()
        .AutoRefresh(true)
        .Spinner(Spinner.Known.Arc)
        .StartAsync("[yellow]Pricing catalog[/]", async ctx =>
    {
        foreach (var product in catalog.Products)
        {
            var response = await aggregator.Send<decimal?>(product.Id);
            evaluations.Add(new Evaluation(product, response));
        }
    });

    return evaluations;
}

// Pretty print.

static void PrettyPrint(IReadOnlyList<Evaluation> evaluations)
{
    var table = new Table()
        .HorizontalBorder()
        .BorderStyle("steelblue1")
        .AddColumn(new TableColumn("Product").LeftAligned())
        .AddColumn(new TableColumn("Price").RightAligned())
        .AddColumn(new TableColumn("Supplier").RightAligned());

    var isFirstEvaluation = true;

    foreach (var evaluation in evaluations)
    {
        if (!isFirstEvaluation)
            table.AddEmptyRow().AddEmptyRow();

        if (IsProductOutOfStock(evaluation))
        {
            table.AddRow(
                new Markup(evaluation.Product.Name).LeftAligned(),
                new Markup("[red]Out of stock[/]").RightAligned());
        }
        else if (IsPriced(evaluation))
        {
            var resultsWithPrice = evaluation.Response.Completed.Where(x => x.Result.HasValue).ToArray();
            var bestPrice = resultsWithPrice.Min(x => x.Result!.Value);

            var isFirstPrice = true;

            foreach (var invocation in resultsWithPrice.OrderBy(x => x.Duration))
            {
                var productName = isFirstPrice ? evaluation.Product.Name : string.Empty;
                var supplierName = invocation.RecipientName;
                var supplierPrice = invocation.Result!.Value;
                var isBestPrice = supplierPrice == bestPrice;
                var resultColor = isBestPrice ? "green3_1" : "red";

                table.AddRow(
                    new Markup(productName).LeftAligned(),
                    new Markup($"[{resultColor}]${supplierPrice}[/]").RightAligned(),
                    new Markup($"[{resultColor}]{supplierName}[/]").RightAligned());

                isFirstPrice = false;
            }
        }

        isFirstEvaluation = false;
    }

    AnsiConsole.Render(table);
}

static bool IsPriced(Evaluation evaluation)
{
    if (!evaluation.Response.Completed.Any())
        return false;

    return evaluation.Response.Completed.Any(x => x.Result is not null);
}

static bool IsProductOutOfStock(Evaluation evaluation)
{
    if (!evaluation.Response.Completed.Any())
        return true;

    return evaluation.Response.Completed.All(x => x.Result is null);
}

// Config.

static RecipientsCollection CollectRecipients()
{
    var collection = new RecipientsCollection();
    collection.Add<AlibabaSupplier>("Alibaba");
    collection.Add<AmazonSupplier>("Amazon");
    collection.Add<WalmartSupplier>("Walmart");
    return collection;
}

record Evaluation(Product Product, AggregatedResponse<decimal?> Response);
