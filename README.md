# Routinely

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![Coverage](https://img.shields.io/badge/coverage-93%25-green.svg)](Routinely.UnitTests/)

High-performance async/await, zero-allocation, cooperatively scheduled coroutines for .NET

Routinely is a lightweight coroutine library designed for game engines and other applications. 
It provides explicit tick-based control flow, achieving high performance with minimal overhead while maintaining zero GC pressure.

---

## Familiar async/await syntax

```csharp
async Coroutine Work()
{
    // Resume next tick
    await Coroutine.Yield();  

    // Await another coroutine
    await OtherWork();  

    // Await a coroutine with a result
    var result = await ResultWork(); 
}
```
## Installation

### NuGet
```
dotnet add package Routinely --prerelease
```
---

## Quick Start

### Basic Usage

```csharp
using Routinely;

var isRunning = true;

var helloWorldCo = HelloWorld();

// Application loop
while (isRunning)
{
    // Tick all coroutines
    Coroutine.ResumeAll();

    // Exit loop when all coroutines have completed
    if(Coroutine.Count == 0)
    {
        isRunning = false;  
    }
}

// Simple coroutine
async Coroutine HelloWorld()
{
    Console.WriteLine("Hello...");

    // Resume next tick
    await Coroutine.Yield();  

    Console.WriteLine("...World!");
}
```

### Fire-and-Forget

```csharp
async Coroutine Work()
{
    for (int i = 0; i < 10; i++)
    {
        await Coroutine.Yield();
    }
}

// Start without tracking
Work().Forget();
```

### Coroutines with Results

```csharp
async Coroutine<int> Work()
{
    int total = 0;

    for (int i = 0; i < 100; i++)
    {
        total += 1;
        await Coroutine.Yield();
    }

    return total;
}
```

---

## Cancellation

Routinely is built around fast immediate cancellation. Coroutines that don't access long lived resources can simply be canceled.

### 1. Cancelation of simple coroutines (Fast, but assumes no cleanup required)

```csharp
async Coroutine Work()
{
    // Loop forever until cancelled
    while(true)
    {
        await Coroutine.Yield();
    }
}

var work = Work();

// Immediately stops the coroutine
work.Cancel();  
```

### 2. Cancelation of coroutines that require resource cleanup.

```csharp
async Coroutine Work()
{
    // Enter a cancellation contract scope
    using var contract = await CancellationContract.Enter();

    try
    {
        // Get some long-lived resource that needs cleanup
        AcquireResource();

        // Do work that may be cancelled
        await Coroutine.Yield().Enforce(contract);

        // Do more work that may be cancelled
        await Coroutine.Yield().Enforce(contract);

        // Await a nested coroutine
        await NestedWork().Enforce(contract);
    }
    finally
    {
        // Always runs, even if cancelled
        ReleaseResource();
    }
}

var work = Work();
work.Cancel();  // Triggers cancellation finally block ensures cleanup
```

### 3: Async cleanup during cancellation

```csharp
async Coroutine Work()
{
    using var contract = await CancellationContract.Enter();

    try
    {
        await OtherWork().Enforce(contract);
        await MoreWork().Enforce(contract);

    }
    catch (CancellationException)
    {
        // Run async cleanup that outlives this coroutine
        // This gets its own execution stack, allowing it to continue even after this coroutine is cancelled
        await WorkCanceled();
    }
}
```
---

## WhenAll - Wait for all to finish

### 1 - Wait for multiple coroutines to complete

```csharp
async Coroutine BigWork()
{
    var workCos = new[]
    {
        WorkA(),
        WorkB(),
        WorkC()
    };

    await Coroutine.WhenAll(workCos);
}
```

### 1 - Wait for multiple coroutines to complete with results (allocates array for results)
```csharp
async Coroutine<int[]> BigWorkWithResults()
{
    var workCos = new[]
    {
        WorkA(),
        WorkB(),
        WorkC()
    };

    var results = await Coroutine.WhenAll(workCos);
    return results;
}
```

### 2 - WhenAll non-allocating overload with pre-allocated array
```csharp
var workCos = new[]
{
    WorkA(),
    WorkB(),
    WorkC()
};

var results = new int[workCos.Length];

async Coroutine BigWorkNonAlloc(Coroutine<int>[] workCos, int[] results)
{
    await Coroutine.WhenAll(workCos, results);
}
```

## WhenAny - First to finish

### 1 - Wait for the first coroutine to complete
```csharp
async Coroutine Race()
{
    var requests = new[]
    {
        RaceA(),
        RaceB(),
        RAceC()
    };

    await Coroutine.WhenAny(requests);
}
```

### 2 - Wait for the first coroutine to complete with result
```csharp
async Coroutine<(int index, int result)> RaceWithResults()
{
    var requests = new[]
    {
        RaceA(),
        RaceB(),
        RAceC()
    };

    var fastestCo = await Coroutine.WhenAny(requests);

    return await fastestCo;  // Await the winning coroutine to get its result
}
```
---

## SwitchTo - Coroutine tail call recursion without stack growth
### 1 - Switch to another coroutine allocating closure via method group
```csharp
async Coroutine StateA()
{
    // Do some work in StateA...
    // Switch to StateB on the next tick
    await Coroutine.SwitchTo(StateB);
}

async Coroutine StateB()
{
    // Do some work in StateB...
    // Switch to StateA on the next tick
    await Coroutine.SwitchTo(StateA);
}
```

### 2 - Switch to another coroutine passing a context to avoid allocating closure
```csharp
async Coroutine StateA()
{
    // Do some work in StateA...
    // Switch to StateB on the next tick without allocating a closure
    await Coroutine.SwitchTo(this, static @this => @this.StateB());
}

async Coroutine StateB()
{
    // Do some work in StateB...
    // Switch to StateA on the next tick without allocating a closure
    await Coroutine.SwitchTo(this, static @this => @this.StateA());
}
```

### 3 - Managing infinite recursion with cancellation

```csharp
async Coroutine StateA()
{
    await Coroutine.SwitchTo(this, static @this => @this.StateB());
}

async Coroutine StateB()
{
    await Coroutine.SwitchTo(this, static @this => @this.StateA());
}

// Will bounce back and forth between StateA and StateB indefinitely on ResumeAll
var stateCo = StateA();

// Cancels the current coroutine state, preventing infinite recursion and allowing cleanup if needed.
// The stateCo handle will always show as IsCompleted = false until it gets cancelled or a switched to coroutine returns.
stateCo.Cancel(); 

```

`SwitchTo` is a terminal operation. The coroutine callstack prior to the `SwitchTo` call is freed and will never resume. This means that any code after the
`SwitchTo` call will never execute.


```csharp
async Coroutine Work()
{
    // Switches but will never receive a result.
    await Coroutine.SwitchTo(VoidWork);

    // Unreachable code - the callstack is freed after SwitchTo and will never resume here
    await Coroutine.Yield();
}
```

Currently only supports switching between `Coroutine` to `Coroutine` (no generic overloads yet). While you can switch from a 
`Coroutine<T>` to a `Coroutine` you won't be able to return a result. 

```csharp
async Coroutine<int> IntWork()
{
    // Switches to a coroutine but can't return an int result
    await Coroutine.SwitchTo(VoidWork);

    // Unreachable code - the callstack is freed after SwitchTo and will never resume here
    return 1;
}
```

Type safety of switching between `Coroutine<T1>` and `Coroutine<T2>`is not enforced by the compiler.

```csharp
async Coroutine<string> StringWork()
{
    await Coroutine.Yield();
    return "Hello";
}

async Coroutine<int> IntWork()
{
    // Switches to a Coroutine<string> but the compiler won't catch this, it will still work however
    await Coroutine.SwitchTo(StringWork);

    // Unreachable code - the callstack is freed after SwitchTo and will never resume here
    return 1;
}
```

TLDR: When using `SwitchTo` with non void coroutines, expect the unexpected!

## FromTask/FromValueTask - Task interop

### 1 - Awaiting a Task/ValueTask from a coroutine.

```csharp
async Task TaskWork()
{
    // Async work
}

async ValueTask ValueTaskWork()
{
    // Async work
}

async Coroutine Work()
{
    await Coroutine.FromTask(TaskWork());
    await Coroutine.FromValueTask(ValueTaskWork());
}
````

### 2 - Awaiting Task/ValueTask from a coroutine with cancellation support.
```csharp
async Task CancellableTaskWork(CancellationToken token)
{
    // Async work that supports cancellation
}

async ValueTask CancellableValueTaskWork(CancellationToken token)
{
    // Async work that supports cancellation
}

async Coroutine Work()
{
    await Coroutine.FromTask(ct => CancellableTaskWork(ct));
    await Coroutine.FromValueTask(ct => CancellableValueTaskWork(ct));
}

var workCo = Work();
workCo.Cancel();
```

The `Cancel` overload that provides a cancellation token will hook into the cancellation contract pattern under the hood.
This means that if the coroutine is cancelled while awaiting a task from `FromTask` or `FromValueTask` the task gets cancelled.

**⚠️ IMPORTANT**: Awaiting a coroutine from a task or value task will cause unexpected behavior and is not supported.

## Coroutine Contexts

Coroutine contexts can be created to allow executing coroutines to run in the current context and to
switch their execution context mid-execution where necessary. The first context created wraps the default
coroutine context (the one used by `ResumeAll` if no initial contexts are created).

### 1 - Creating and switching between multiple coroutine contexts
```csharp
var highPriority = Coroutine.CreateContext();
var lowPriority = Coroutine.CreateContext();

var lowPriorityCounter = 0;
var isRunning = true;

while(isRunning)
{
    Coroutine.SetContext(highPriority);

    // Create a new coroutine in the high priority context
    Work().Forget();

    Coroutine.ResumeAll();

    // Execute every 5 ticks
    if(lowPriorityCounter++ % 5 == 0)
    {
        Coroutine.SetContext(lowPriority);
     
        // Create a new coroutine in the low priority context
        Work().Forget();

        Coroutine.ResumeAll();
    }
}

async Coroutine Work()
{
    await Coroutine.Yield();
}

```

### 2 - Switching the context of an executing coroutine
```csharp

var highPriority = Coroutine.CreateContext();
var lowPriority = Coroutine.CreateContext();

async Coroutine VariabePriorityWork()
{
    var count = 0;

    // Force the coroutine to the high priority context when first executed
    await Coroutine.Context(highPriority);

    while(count++ < 5)
    {
        await Coroutine.Yield();
    }

    // Force to the low priority context after 5 ticks
    await Coroutine.Context(lowPriority);

    await Coroutine.Yield();
}

```

**⚠️ IMPORTANT**: If a coroutine awaits the current context the `await Coroutine.Context(...)` 
call will complete synchronously and the coroutine will continue to the next await. Changing a coroutine's
context moves the entire callstack from the current context to the new context. 
If a coroutine switches context multiple times the callstack will move back and forth between contexts with each switch.

## Things to be aware of!
* A coroutine cannot directly await a Task. Use Coroutine.FromTask() instead. Direct await will cause a build failure as framework task like types can't match the interface requirements
of `ICoroutineNotifyAwaiting`.
* Coroutines are designed to be short lived handles to cooperatively scheduled work. The library aggressively recycles resources used to maintain 
a coroutine's execution state (e.g. it's callstack, core state, async state machine and results/exceptions). By default they will persist for one tick after completion
before being recycled. If `Forget` is used the coroutine will be recycled immediately after completion.
* When a coroutine awaits another coroutine, the awaited coroutine becomes part of the callstack of the awaiting coroutine. This means that
a coroutine can only be awaited once. Attempting to await it a second time is an exception condition.
* If a coroutine returns false for `HasContext` all other state properties of that coroutine will also return false. Awaiting a coroutine that
`HasContext == false` is an exception condition.
* The library currently only supports running on the main thread. Attempting to run coroutines
on a worker thread will cause unexpected behavior. Support for running on worker threads is planned for a future release.

## Benchmarks ##

The `Routinely.BenchMark` project tests coroutine execution across various load profiles:
- **Simple execution**: 1 → 1K → 10K → 100K coroutines
- **Nested coroutines**: Multi-level call stacks
- **Coroutines with results**: Generic return values
- **WhenAll/WhenAny coordination**: Concurrent operations
- **SwitchTo recursion**: Tail call scenarios without stack growth

Some highlights from the benchmarks for single coroutine life cycle (create → yield -> complete) at different scales:

| Method                               | Coroutines | Mean              | Error           | StdDev          | Gen0     | Gen1     | Gen2     | Allocated |
|------------------------------------- |----------- |------------------:|----------------:|----------------:|---------:|---------:|---------:|----------:|
| Single_Async                         | 1          |         35.115 ns |       0.1519 ns |       0.1421 ns |        - |        - |        - |         - |
| Single_Async                         | 1000       |     36,676.678 ns |      41.1863 ns |      38.5256 ns |        - |        - |        - |         - |
| Single_Async                         | 10000      |    371,381.077 ns |     689.1385 ns |     644.6205 ns |        - |        - |        - |         - |
| Single_Async                         | 100000     |  3,982,885.397 ns |   3,583.8487 ns |   2,992.6760 ns |        - |        - |        - |         - |

There is a [Results](Routinely.BenchMark/Results/RYZEN-9950X3D.md) folder in the benchmark project with detailed results for all benchmarks. These benchmarks were run on a
Ryzen 9950x3d processor. Will add more on different hardware in the future.

## AOT Compatibility
Routinely is fully compatible with AOT compilation.

## Roadmap

### v0.1.0-alpha (Current)
- ✅ Core coroutine runtime (async/await)
- ✅ Zero-allocation execution
- ✅ SwitchTo for tail call recursion without stack growth
- ✅ Contract-based cancellation with cleanup
- ✅ WhenAll/WhenAny coordination
- ✅ Forget() fire-and-forget pattern
- ✅ Task/ValueTask interop
- ✅ 93% test coverage

### v0.2.0 (Planned)
- [ ] `Coroutine<TResult>` support for `SwitchTo`
- [ ] Roslyn analyzers for common pitfalls (`SwitchTo` misuse, awaiting non-context coroutines, etc.)
- [ ] Support for running on worker threads
---

## Contributing

Contributions are welcome!

```bash
# Run tests
dotnet test

# Run benchmarks
cd Routinely.BenchMark
dotnet run -c Release

# Run code coverage
cd Routinely.UnitTests
.\run-coverage.ps1
```

---

## License

MIT License - see [LICENSE](LICENSE) file for details.

---

Questions? Open an issue or start a discussion!
