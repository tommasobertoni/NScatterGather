using System;
using System.Collections.Generic;
using System.Text.Json;
using NScatterGather;
using NScatterGather.Recipients;
using NScatterGather.Samples.TaskParallelization;

var recipients = CollectRecipients();
var aggregator = new Aggregator(recipients);

string id = "001";
var response = await aggregator.Send(id);

var vm = new ViewModel { Id = id };

foreach (var completed in response.Completed)
{
    var result = completed.Result;

    switch (result)
    {
        case EmployeeData employeeData:
            MapEmployeeTo(employeeData, vm);
            break;

        case OrganizationData organizationData:
            MapOrganizationTo(organizationData, vm);
            break;

        case PortfolioData portfolioData:
            MapPortfolioTo(portfolioData, vm);
            break;
    }
}

var json = JsonSerializer.Serialize(vm, new JsonSerializerOptions { WriteIndented = true });

Console.WriteLine(json);

// Mapping.

static void MapEmployeeTo(EmployeeData employeeData, ViewModel vm)
{
    vm.FullName = employeeData.FullName;
    vm.Age = employeeData.Age;
}

static void MapOrganizationTo(OrganizationData organizationData, ViewModel vm)
{
    vm.Company = organizationData.Company;
    vm.Teams = organizationData.Teams;
}

static void MapPortfolioTo(PortfolioData portfolioData, ViewModel vm)
{
    vm.Projects = portfolioData.Projects;
    vm.Repositories = portfolioData.Repositories;
}

static RecipientsCollection CollectRecipients()
{
    var collection = new RecipientsCollection();
    collection.Add<EmployeeService>();
    collection.Add<OrganizationService>();
    collection.Add<PortfolioService>();
    return collection;
}

class ViewModel
{
    public string? Id { get; set; }

    public string? FullName { get; set; }

    public int? Age { get; set; }

    public Company? Company { get; set; }

    public IReadOnlyList<Team>? Teams { get; set; }

    public IReadOnlyList<Project>? Projects { get; set; }

    public IReadOnlyList<Repository>? Repositories { get; set; }
}
