using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NScatterGather;
using NScatterGather.Recipients;
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

    AnsiConsole.Render(new Markup("[silver italic]Pricing:[/] "));

    foreach (var product in catalog.Products)
    {
        AnsiConsole.Render(new Markup($"[silver italic]{product.Name}, [/]"));

        var response = await aggregator.Send<string, decimal?>(product.Id);
        evaluations.Add(new Evaluation(product, response));
    }

    AnsiConsole.WriteLine();

    return evaluations;
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
                var supplierName = GetSupplierName(invocation.RecipientType);
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

static string GetSupplierName(Type recipientType) =>
    recipientType.Name.Replace("Supplier", string.Empty);

// Config.

static RecipientsCollection CollectRecipients()
{
    var collection = new RecipientsCollection();
    collection.Add<AlibabaSupplier>();
    collection.Add<AmazonSupplier>();
    collection.Add<WalmartSupplier>();
    return collection;
}

record Evaluation(Product Product, AggregatedResponse<decimal?> Response);
