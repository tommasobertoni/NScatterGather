using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NScatterGather.Samples.TaskParallelization
{
    class PortfolioService
    {
        public Task<PortfolioData?> GetPortfolio(string id)
        {
            var abc = id switch
            {
                "001" => new PortfolioData(
                    new List<Project>
                    {
                        new Project("Apple monitor", DateTime.Parse("05/08/2010")),
                        new Project("Apple collector", DateTime.Parse("07/08/2010")),
                        new Project("Moonshine", DateTime.Parse("11/02/2001")),
                    },
                    new List<Repository>
                    {
                        new Repository("Apple.Monitor.Core", "https://github.com/Contoso/Apple.Monitor.Core"),
                        new Repository("Apple.Monitor.UI", "https://github.com/Contoso/Apple.Monitor.UI"),
                        new Repository("Apple.Collector", "https://github.com/Contoso/Apple.Collector"),
                        new Repository("MoonShine", "https://tfs.contoso.org/tfs/CNTS/moonshine"),
                    }),

                _ => null,
            };

            return Task.FromResult(abc);
        }
    }

    class PortfolioData
    {
        public IReadOnlyList<Project> Projects { get; }

        public IReadOnlyList<Repository> Repositories { get; }

        public PortfolioData(
            IReadOnlyList<Project> projects,
            IReadOnlyList<Repository> repositories)
        {
            Projects = projects;
            Repositories = repositories;
        }
    }

    class Project
    {
        public string Name { get; }

        public DateTime Started { get; }

        public Project(
            string name,
            DateTime started)
        {
            Name = name;
            Started = started;
        }
    }

    class Repository
    {
        public string Name { get; }

        public string Url { get; }

        public Repository(
            string name,
            string url)
        {
            Name = name;
            Url = url;
        }
    }
}
