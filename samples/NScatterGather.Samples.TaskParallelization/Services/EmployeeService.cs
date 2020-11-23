
namespace NScatterGather.Samples.TaskParallelization
{
    class EmployeeService
    {
        public EmployeeData? GetEmployeeById(string id)
        {
            return id switch
            {
                "001" => new EmployeeData("Jack Daniels", 48),
                _ => null,
            };
        }
    }

    class EmployeeData
    {
        public string? FullName { get; }

        public int Age { get; }

        public EmployeeData(
            string fullName,
            int age)
        {
            FullName = fullName;
            Age = age;
        }
    }
}
