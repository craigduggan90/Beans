# Beans.Requestable

A lightweight mediator/CQRS project that exists because I keep reaching for the same pattern.

I've worked on many projects which used [MediatR](https://github.com/jbogard/MediatR) - but rarely any that have gone 
beyond a simple CQRS use-case.  With that package moving to a commercial license, I decided to write a little project 
to handle that scenario.

## Getting Started

`AddRequestableServices` registers `IMediator` and every request handler in the calling assembly.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRequestableServices();
```

If your handlers live elsewhere or you want something other than the defaults, you can pass a configuration action:

```csharp
builder.Services.AddRequestableServices(options =>
{
    options.Assemblies = [typeof(MyHandler).Assembly, Assembly.GetExecutingAssembly()];
    options.MediatorImplementationType = typeof(MyCustomMediator);
    options.MediatorServiceLifetime = ServiceLifetime.Transient;
});
```

Both overloads return the service collection for chaining with other registrations.

- **`Assemblies`** - the assemblies to search for `IRequestHandler<>` and `IRequestHandler<,>` implementations.  Handlers 
  are registered as transient, and abstract and open generic types are skipped. Setting `Assemblies` replaces the default
  value (default: `Assembly.GetCallingAssembly()`).
- **`MediatorImplementationType`** - your own `IMediator`, for custom routing or to wrap the default one.  It must 
  implement `IMediator` or registration will throw an `ArgumentException` (default: `Beans.Requestable.Mediator`).
- **`MediatorServiceLifetime`** - the lifetime of the `IMediator` registration (default: transient).

## Requests and Handlers

A request implements `IRequest` or `IRequest<TResponse>` and represents a query or a command.  A request that implements
`IRequest` without a response type may be referred to as a "void request".

```csharp
public sealed record GetProductQuery(Guid Id) : IRequest<Product?>;

public sealed record DeleteProductCommand(Guid Id) : IRequest;
```

Each request needs a handler. Handlers are resolved from the container, so they can take dependencies through their 
constructors:

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

Build your request and call `SendAsync`:

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

The mediator resolves handlers from the DI container, so a handler that depends on a scoped service 
(e.g. a `DbContext`) gets the same instance as the controller that sent the request.

## Exceptions

`RequestableException` is thrown by the default `IMediator` when it fails to resolve a handler for the request, and by 
the registration extension when two classes handle the same request type.
