
namespace NScatterGather.Samples.CompetingTasks
{
    class Product
    {
        public string Id { get; }

        public string Name { get; }

        public Product(
            string id,
            string name)
        {
            Id = id;
            Name = name;
        }
    }
}
