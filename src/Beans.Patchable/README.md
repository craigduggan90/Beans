# Beans.Patchable

This project exists because I found `JsonPatchDocument` fiddly to work with, and the alternatives I have tried kept 
tripping over the same problem: a PATCH request body has three states for any given field: not set, set null, and set 
value. Every other way I tried could only get me two out of three, which ain't bad, but isn't quite good enough. 

## Getting Started

Request models for patch endpoints should represent patchable values with `Optional<T>`.

```csharp
public sealed record ProductPatchRequest(
    Optional<string> Name,
    Optional<string?> Description,
    Optional<decimal> Price);
```

These can be passed directly to the `UpdateProperty`.  If the value is assigned, this will return true - if you want to 
handle all mutations in one method, it also sets `IsDirty` to true, so you'll know if any value has been assigned.

```csharp
public sealed class Product : PatchableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    
    public void Update(
        Optional<string> name,
        Optional<string?> description,
        Optional<decimal> price)
    {
        UpdateProperty(nameof(Name), name);
        UpdateProperty(nameof(Description), description);
        UpdateProperty(nameof(Price), price);
    }
}


```

Both Minimal API's and Controller-Based API's need `OptionalJsonConverterFactory` registered on the 
`JsonSerializerOptions` that binds the request body. 

> [!NOTE]
> In the below examples, `IProductStore` is just a placeholder persistence service.

### Minimal API's

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new OptionalJsonConverterFactory()));

builder.Services.AddSingleton<IProductStore, ProductStore>();

var app = builder.Build();

app.MapPatch("/products/{id:guid}", (Guid id, ProductPatchRequest request, IProductStore store) =>
{
    var product = store.Find(id);
    if (product is null)
        return Results.NotFound();

    product.UpdateProperty(nameof(Product.Name), request.Name);
    product.UpdateProperty(nameof(Product.Description), request.Description);
    product.UpdateProperty(nameof(Product.Price), request.Price);

    if (product.IsDirty)
        store.Save(product);

    return Results.NoContent();
});

app.Run();
```

### Controller-Based API's

```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new OptionalJsonConverterFactory()));
```

```csharp
[ApiController]
[Route("products")]
public sealed class ProductsController(IProductStore store) : ControllerBase
{
    [HttpPatch("{id:guid}")]
    public IActionResult Patch(Guid id, ProductPatchRequest request)
    {
        var product = store.Find(id);
        if (product is null)
            return NotFound();

        product.UpdateProperty(nameof(Product.Name), request.Name);
        product.UpdateProperty(nameof(Product.Description), request.Description);
        product.UpdateProperty(nameof(Product.Price), request.Price);

        if (product.IsDirty)
            store.Save(product);

        return NoContent();
    }
}
```

Both endpoints call the same `UpdateProperty` sequence on `Product`, so behaviour is identical regardless of which API
style is in use.

### Writing Optional&lt;T&gt;

With just `OptionalJsonConverterFactory` registered, an unset `Optional<T>` still serializes — as `null`, same as an explicitly-null one:

```csharp
JsonSerializer.Serialize(new ProductPatchRequest(Optional<string>.Unset, Optional<string?>.Unset, Optional<decimal>.Unset), options);
// {"Name":null,"Description":null,"Price":0}
```

To omit unset properties from the serialized JSON, register `OptionalJsonTypeInfoModifier.OmitUnsetOptionals` as a 
contract modifier on the same options:

```csharp
var options = new JsonSerializerOptions
{
    Converters = { new OptionalJsonConverterFactory() },
    TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        .WithAddedModifier(OptionalJsonTypeInfoModifier.OmitUnsetOptionals)
};
```

## Types

### PatchableEntity

`PatchableEntity` is the base class for entities that support `UpdateProperty`.  

Whenever a property is successfully patched (whether setting a value or `null`), `SetDirty()` is called to mark the 
object as changed, setting `IsDirty` to true.  Once it is set to true, `IsDirty` cannot be returned to false for that
instance.

`PatchableEntity` implements `IPatchableEntity` - any code that only needs to check `IsDirty` or call `SetDirty()` can 
depend on this interface instead.

#### SetDirty

The `SetDirty()` method is intentionally public.  `UpdateProperty` only detects changed property references; mutating 
something via a method on that instance (e.g. `List<T>.Add(T)`) does not change the reference of the property.

Where this is the case, `SetDirty()` can be called directly to mark the object as changed.

You can also override `SetDirty()` to record something like a last-modified timestamp:

```csharp
public override void SetDirty()
{
    LastModified = DateTimeOffset.UtcNow;
    base.SetDirty();
}
```

Always call `base.SetDirty()` - `IsDirty` is read-only outside `PatchableEntity`, so it's the only way to actually set it.

### Optional&lt;T&gt;

`Optional<T>` represents a value that may or may not have been supplied, distinguishing three states:

| Value State | Optional                       | `IsSet` | `Value`          |
|-------------|--------------------------------|---------|------------------|
| Not Set     | `Optional<string>.Unset`       | `false` | `null` (default) |
| Value       | `Optional<string>.Of("Hello")` | `true`  | `"Hello"`        |
| Null        | `Optional<string?>.Of(null)`   | `true`  | `null`           |

`Value` returns `default` when unset, so an unset optional and one explicitly set to `null` have the same `Value`. 
Use `IsSet` or `TryGetValue` when that distinction matters:

```csharp
if (optional.TryGetValue(out var value))
{
    // value was explicitly supplied (but might still be null!)
}
```

`ValueType` exposes the declared type `T`, which is useful when `Value` is `null` and therefore has no runtime type of 
its own.

## Serialization

Three types work together to make `Optional<T>` behave correctly with `System.Text.Json`:

| Type                           | Purpose                                                                                                                                         |
|--------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------|
| `OptionalJsonConverter<T>`     | Reads and writes a single `Optional<T>`.                                                                                                        |
| `OptionalJsonConverterFactory` | Creates an `OptionalJsonConverter<T>` for any `T`.                                                                                              |
| `OptionalJsonTypeInfoModifier` | Optional contract modifier that omits unset `Optional<T>` properties from serialized JSON. See [Writing Optional&lt;T&gt;](#writing-optionalt). |

## Test Strategy

- **`Beans.Patchable.UnitTests`** covers `Optional<T>`, `PatchableEntity`, `PatchableException`, and the serialization 
  types in isolation.
- **`Beans.Patchable.IntegrationTests`** creates two real ASP.NET Core hosts, one minimal API, one MVC controller, 
  sharing a single service, and runs the same patch scenarios against both to catch any behavioural differences.

```bash
dotnet test
```

(`global.json` opts the repo into the newer Microsoft.Testing.Platform-based `dotnet test` experience that xunit v3 uses.)
