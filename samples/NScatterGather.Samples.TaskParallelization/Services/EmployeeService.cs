
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

    record EmployeeData(string? FullName, int Age);
}
