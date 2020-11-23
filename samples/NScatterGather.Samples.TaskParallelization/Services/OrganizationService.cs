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

    class OrganizationData
    {
        public Company Company { get; }

        public IReadOnlyList<Team> Teams { get; }

        public OrganizationData(
            Company company,
            IReadOnlyList<Team> teams)
        {
            Company = company;
            Teams = teams;
        }
    }

    class Company
    {
        public string Name { get; }

        public DateTime Founded { get; }

        public Company(
            string name,
            DateTime founded)
        {
            Name = name;
            Founded = founded;
        }
    }

    class Team
    {
        public string Name { get; }

        public int Size { get; }

        public DateTime Created { get; }

        public Team(
            string name,
            int size,
            DateTime created)
        {
            Name = name;
            Size = size;
            Created = created;
        }
    }
}
