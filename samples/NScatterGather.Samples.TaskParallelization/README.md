
# Task Parallelization

![task-parallelization-diagram](../../assets/images/task-parallelization-diagram.png)

This sample shows a task parallelization scenarion, in which different `Service`s provide their own data for a given employee `id`.

The result of the scatter-gather operation is used to create a composite model containing all the data received.

```csharp
class ViewModel
{
    // Unique identifier for the employee,
    // shared across services

    public string? Id { get; set; }

    // from EmployeeService

    public string? FullName { get; set; }

    public int? Age { get; set; }

    // from OrganizationService

    public Company? Company { get; set; }

    public IReadOnlyList<Team>? Teams { get; set; }

    // from PortfolioService

    public IReadOnlyList<Project>? Projects { get; set; }

    public IReadOnlyList<Repository>? Repositories { get; set; }
}
```
