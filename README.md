<p align="center">
  <image src="assets/logo/nscattergather-logo-full.png" alt="NScatterGather logo" width="600px">
</p>

![License MIT](https://img.shields.io/github/license/tommasobertoni/NScatterGather)
![CI](https://github.com/tommasobertoni/NScatterGather/workflows/CI/badge.svg?branch=master)

<details>
<summary>Table of Contents</summary>

[Intro](#Intro)<br/>
[When to use](#When-to-use)<br/>
[How to use](#How-to-use)<br/>
[Special cases](#Special-cases)<br/>
[Samples](#Samples)<br/>
</details>

<br/>

# Intro

The Scatter-Gather pattern: send a request message to multiple recipients, and then aggregate the results into a single response.

![scatter-gather-diagram](assets/images/scatter-gather-diagram.png)

This pattern helps to limit the coupling between the consumer and the recipients in integration scenarios, and provides standard error-handling and timeout capabilities.

# When to use

## Competing Tasks

The recipients compete in order to provide the best, or the fastest, response to the request. The consumer will then pick the best value from the aggregated response.

_e.g._ Get an item's price from a collection of suppliers:

![competing-tasks-diagram](assets/images/competing-tasks-diagram.png)

## Task parallelization

Different operations are computed concurrently, and their results combined or used together. The result types could be different.

_e.g._ Get a user's data from different services, and then compose a view model:

![tasks-parallelization-diagram](assets/images/tasks-parallelization-diagram.png)

# How to use
Use a `RecipientsCollection` to register the eligible recipients:
```csharp
var collection = new RecipientsCollection();
collection.Add<Foo>();
collection.Add(new Bar());
```

Use an `Aggregator` for sending the requests to all the available recipients that can support the desired request/response types, and for aggregating the results:
```csharp
var aggregator = new Aggregator(collection);

// Send a request to all the recipients
// capable of accepting and int.
// The results are then combined in the response:
AggregatedResponse<object> objects = await aggregator.Send(42);

// The following overload can be used when
// the return type is either known or binding:
AggregatedResponse<string> strings = await aggregator.Send<int, string>(42);

// In the second case, only the recipients that accept
// an int and return a string will be invoked.
```

Inspect the `AggregatedResponse` containing the results of the scatter-gather operation, grouped by completed, faulted and incomplete:
```csharp
var completed = response.Completed[0];
// (Type recipientType, string result) = completed;

var faulted = response.Faulted[0];
// (Type recipientType, Exception ex) = faulted;

var incomplete = response.Incomplete[0];
// Type recipientType = incomplete;
```

The recipients for a request, and optionally a response type, are identifyed via reflection: **no binding contracts** are used _(e.g. `IRecipient`)_.<br/>
This allows for less friction in both the implementation and the maintenance of the integrations, and, furthermore, to identify the target methods by conventions (i.e. sync/async).

# Special cases

## Handling async methods

The `Aggregator` exposes async-only methods for sending requests.

By convention, even if the consumer requested only results of type `TResponse`, a recipient that returns `Task<TResponse>` (or `ValueTask<TResponse>`) will still be invoked and its result awaited:

```csharp
class Foo { public int Echo(int n) => n; }
class Bar { public Task<int> EchoAsync(int n) => Task.FromResult(n); }

// Nothing changes!
var response = await aggregator.Send(42);
// [ 42, 42 ]
```

## Handling conflicts

Sometimes, a recipient can have two or more methods conflicting, given a request type:
```csharp
class Foo
{
    public int Double(int n) => n * 2;
    public long Triple(int n) => n * 3L;
}
```

In this case, the aggregator will be able to invoke the recipient only if the return type of the conflicting methods is different, and it's explicitely defined by the consumer:
```csharp
// The recipient won't be used.
_ = await aggregator.Send(42);

// Method "Triple" will be invoked.
var response = await aggregator.Send<int, long>(42);
```

# Samples

### Hello world
```csharp
class Foo { public int Double(int n) => n * 2; }
class Bar { public long Square(int n) => n * 1L * n; }

var response = await aggregator.Send(42);
// [ 84, 1764L ]
```

### Specify the response type
```csharp
class Foo { public string Stringify(int n) => n.ToString(); }
class Bar { public long Longify(int n) => n * 1L; }

var onlyStrings = await aggregator.Send<int, string>(42);
// [ "42" ]
```

### Invoke async methods
```csharp
class Foo { public string Stringify(int n) => n.ToString(); }

class Bar
{
    public Task<long> Longify(int n)
    {
        await Task.Yield();
        return n * 1L;
    }
}

var response = await aggregator.Send(42);
// [ "42", 42L ]
```

### Error handling
```csharp
class Foo
{
    public string Todo(string s) =>
        throw new NotImplementedException("TODO");
}

var response = await aggregator.Send("Don't Panic");
var (recipientType, exception) = response.Faulted[0];
// ( typeof(Foo), NotImplementedException("TODO") )
```

### Timeout
```csharp
class Foo
{
    public Task<int> Block(int n)
    {
        var tcs = new TaskCompletionSource<int>();
        return tcs.Task; // It will never complete.
    }
}

var timeout = TimeSpan.FromSeconds(5);
using var cts = new CancellationTokenSource(timeout);

var response = await aggregator.Send(42, cts.Token);
Type recipientType = response.Incomplete[0];
// typeof(Foo)
```

For more, take a look at the [samples project in solution](samples/NScatterGather.Samples).
