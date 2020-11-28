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

    record PortfolioData(IReadOnlyList<Project> Projects, IReadOnlyList<Repository> Repositories);

    record Project(string Name, DateTime Started);

    record Repository(string Name, string Url);
}
