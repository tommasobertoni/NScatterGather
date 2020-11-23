using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using NScatterGather.Recipients;

namespace NScatterGather.Samples.TaskParallelization
{
    public class Program
    {
        static async Task Main()
        {
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
                        MapToViewModel(employeeData, vm);
                        break;

                    case OrganizationData organizationData:
                        MapToViewModel(organizationData, vm);
                        break;

                    case PortfolioData portfolioData:
                        MapToViewModel(portfolioData, vm);
                        break;
                }
            }

            var json = JsonSerializer.Serialize(vm, new JsonSerializerOptions { WriteIndented = true });

            Console.WriteLine(json);
        }

        private static void MapToViewModel(EmployeeData employeeData, ViewModel vm)
        {
            vm.FullName = employeeData.FullName;
            vm.Age = employeeData.Age;
        }

        private static void MapToViewModel(OrganizationData organizationData, ViewModel vm)
        {
            vm.Company = organizationData.Company;
            vm.Teams = organizationData.Teams;
        }

        private static void MapToViewModel(PortfolioData portfolioData, ViewModel vm)
        {
            vm.Projects = portfolioData.Projects;
            vm.Repositories = portfolioData.Repositories;
        }

        private static RecipientsCollection CollectRecipients()
        {
            var collection = new RecipientsCollection();
            collection.Add<EmployeeService>();
            collection.Add<OrganizationService>();
            collection.Add<PortfolioService>();
            return collection;
        }
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
}
