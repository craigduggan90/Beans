# Beans.Requestable

This project exists because I keep wanting the same small slice of the mediator pattern: send a request object, and have 
it routed to the one handler that knows what to do with it.  That is all CQRS needs to get going, and it's all this does 
- no pipelines, no notifications, no behaviours.

## Getting Started

`AddRequestableServices` registers `IMediator` and every request handler it finds in the calling assembly with the 
container.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRequestableServices();
```

If your handlers live somewhere else, or you want something other than the defaults, pass a configuration action:

```csharp
builder.Services.AddRequestableServices(options =>
{
    options.Assemblies = [typeof(MyHandler).Assembly, Assembly.GetExecutingAssembly()];
    options.MediatorImplementationType = typeof(MyCustomMediator);
    options.MediatorServiceLifetime = ServiceLifetime.Singleton;
});
```

Both overloads return the service collection, so they can be chained with other registrations.

| Option                      | Default                                          |
|-----------------------------|--------------------------------------------------|
| `Assemblies`                | The assembly that called `AddRequestableServices` |
| `MediatorImplementationType`| `Mediator`                                       |
| `MediatorServiceLifetime`   | `Transient`                                      |

- **`Assemblies`** - the assemblies to search for `IRequestHandler<>` and `IRequestHandler<,>` implementations.  Handlers 
  are registered as transient, and abstract and open generic types are skipped.  A class can implement any number of 
  handler interfaces, and any other interfaces it likes.
- **`MediatorImplementationType`** - swap in your own `IMediator`, for custom routing or to wrap the default one.  It 
  must implement `IMediator`, or registration throws an `ArgumentException`.
- **`MediatorServiceLifetime`** - the lifetime of the `IMediator` registration.

> [!NOTE]
> If two classes handle the same request type, both are registered and the container resolves the last one.  Keep it to 
> one handler per request.

## Requests and Handlers

A request implements `IRequest` or `IRequest<TResponse>` and represents a query or a command.  It's good practice to 
make requests immutable, so records suit them.

```csharp
public sealed record GetProductQuery(Guid Id) : IRequest<Product?>;

public sealed record DeleteProductCommand(Guid Id) : IRequest;
```

A request that implements `IRequest` on its own, with no response type, is a "void request".

Each request has a handler, which is where the work happens.  Handlers are resolved from the container, so they can take 
dependencies through their constructors:

```csharp
public sealed class GetProductQueryHandler(IProductStore store) : IRequestHandler<GetProductQuery, Product?>
{
    public Task<Product?> HandleAsync(GetProductQuery request, CancellationToken cancellationToken)
        => store.FindAsync(request.Id, cancellationToken);
}

public sealed class DeleteProductCommandHandler(IProductStore store) : IRequestHandler<DeleteProductCommand>
{
    public Task HandleAsync(DeleteProductCommand request, CancellationToken cancellationToken)
        => store.DeleteAsync(request.Id, cancellationToken);
}
```

## Sending Requests

Take `IMediator` as a dependency and call `SendAsync`:

```csharp
[ApiController]
[Route("products")]
public sealed class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var product = await mediator.SendAsync(new GetProductQuery(id), cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.SendAsync(new DeleteProductCommand(id), cancellationToken);
        return NoContent();
    }
}
```

The mediator resolves handlers from the container it was created from, so a handler that depends on a scoped service 
(a `DbContext`, say) gets the same instance as the controller that sent the request.

A request with more than one response type (`IRequest<int>` and `IRequest<string>`, for instance) is routed by the type 
you send it as.

## Exceptions

`RequestHandlerException` is thrown by the default `IMediator` when there is no handler registered for the request.  
Exceptions thrown by a handler are not wrapped, they propagate as they are.

## Test Strategy

- **`Beans.Requestable.UnitTests`** covers the mediator, the exception, and what `AddRequestableServices` registers 
  (including handlers that implement unrelated interfaces, classes that handle several requests, and abstract or open 
  generic types that must be skipped).
- **`Beans.Requestable.IntegrationTests`** runs a real ASP.NET Core host with a controller, and checks requests through 
  it: responses, void requests, handlers sharing the request's scope, missing handlers, and failing handlers.

```bash
dotnet test
```

(`global.json` opts the repo into the newer Microsoft.Testing.Platform-based `dotnet test` experience that xunit v3 uses.)

## Frequently Asked Questions

### What is this for?

Several of my projects used [MediatR](https://github.com/jbogard/MediatR) to help implement CQRS, but none needed or 
fully used what it offers.  With that package moving to a commercial license, I wrote a little helper of my own, which 
started life as [BasicMediator](https://github.com/craigduggan90/BasicMediator).  This is that, now living with the rest 
of the beans.

If you're after handler pipelines, broadcast requests and notifications, do check out Jimmy's package!  The names 
(`IRequest` and `IRequestHandler`) match, so swapping over shouldn't be very difficult.
