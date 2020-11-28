using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NScatterGather.Samples.TaskParallelization
{
    class OrganizationService
    {
        public Task<OrganizationData?> GetOrganizationData(string id)
        {
            var res = id switch
            {
                "001" => new OrganizationData(
                    new Company("Contoso", DateTime.Parse("05/05/1978")),
                    new List<Team>
                    {
                        new Team("Falling apple", 6, DateTime.Parse("01/08/2010")),
                        new Team("New moon", 9, DateTime.Parse("10/02/2001")),
                    }),

                _ => null,
            };

            return Task.FromResult(res);
        }
    }

    record OrganizationData(Company Company, IReadOnlyList<Team> Teams);

    record Company(string Name, DateTime Founded);

    record Team(string Name, int Size, DateTime Created);
}
