# Beans.Common

This project exists because every service I write ends up needing the same handful of small helpers, and I kept copying 
them from whichever project I wrote last. None of it is clever, but each one makes something awkward to test a little 
less awkward, and none of it is worth writing a fourth time.

## Getting Started

`AddCommonServices` registers all services in this project with the container.  At the moment, that is:

| Service                        | Lifetime  |
|--------------------------------|-----------|
| `IEnvironmentVariableAccessor` | Singleton |

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCommonServices();
```

This method returns the service collection, so it can be chained with other registrations.

## Types

### DateTimeOffsetProvider

`DateTimeOffset.Now` is hard to test, because there's no way to tell it what time it is.  `DateTimeOffsetProvider.Now` 
is a drop-in replacement that returns the real time, unless you've told it otherwise.

```csharp
public sealed class Invoice
{
    public DateTimeOffset CreatedAt { get; } = DateTimeOffsetProvider.Now;
}
```

In a test, create a `DateTimeOffsetProviderContext` to fix the time.  The provider returns that timestamp until the 
context is disposed:

```csharp
var fixedTimestamp = new DateTimeOffset(2016, 4, 16, 7, 53, 14, TimeSpan.FromHours(-5));

using (new DateTimeOffsetProviderContext(fixedTimestamp))
{
    var invoice = new Invoice();
    Assert.Equal(fixedTimestamp, invoice.CreatedAt);
}
```

Outside of any context it falls back to `DateTimeOffset.Now`, so you get the local offset rather than UTC.  If you'd 
rather have UTC, `DateTimeOffsetProvider.UtcNow` is the equivalent of `DateTimeOffset.UtcNow`.  With a context active it 
returns the same fixed instant, converted to UTC.

Contexts can be nested - the innermost one wins, and disposing it restores the one before.  They're meant to be used as 
`using` scopes, but disposing them out of order or more than once is safe too: a disposed context is never applied 
again.

The fixed value follows the code rather than the thread: it flows into anything started from inside the scope, so 
`Task.Run` and `await`s that resume on another thread all see it, while concurrent flows each keep their own.

> [!NOTE]
> Two things follow from that:
> - A context created inside an `async` method only lasts until that method returns.  If a helper creates one and the 
>   test body expects to see it, it won't - create the context in the method that needs it.
> - Work started inside the scope keeps the fixed value after the scope ends.  A fire-and-forget task will still see the 
>   fixed time once the `using` block has been disposed.

### GuidProvider

The same idea for `Guid.NewGuid()`.  `GuidProvider.New` returns a fresh `Guid` normally, or the one you fixed with a 
`GuidProviderContext`:

```csharp
var fixedGuid = new Guid("7e0c1088-c1ef-4eff-8711-5638e9d99f4f");

using (new GuidProviderContext(fixedGuid))
{
    Assert.Equal(fixedGuid, GuidProvider.New);
}

Assert.NotEqual(fixedGuid, GuidProvider.New);
```

Nesting and the async behaviour work the same way as for `DateTimeOffsetProvider`.

### IEnvironmentVariableAccessor

Wraps `Environment.GetEnvironmentVariable` and `Environment.SetEnvironmentVariable` behind an interface, so code that 
reads configuration from the environment can take it as a dependency, and tests can substitute it instead of changing 
process-wide state.

| Method                       | Behaviour                                                             |
|------------------------------|-----------------------------------------------------------------------|
| `Get(variable)`              | Returns the value, or `null` if the variable isn't set.               |
| `Set(variable, value)`       | Sets the variable.  Passing `null` removes it.                        |

`EnvironmentVariableAccessor` is the implementation that `AddCommonServices` registers.

## Test Strategy

- **`Beans.Common.UnitTests`** covers each provider with and without a context, including that a context only applies 
  within its `using` scope, runs `EnvironmentVariableAccessor` against real environment variables, and checks what 
  `AddCommonServices` registers.

The context classes are excluded from code coverage.  They're exercised through the providers that use them.

```bash
dotnet test
```
