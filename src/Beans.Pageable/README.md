# Beans.Pageable

This project exists because I kept having to implement date filtering and pagination, and found that I was 
clone-and-owning far too often, with the inevitable flip-flopping of inclusive/exclusive on date filters.

## Getting Started

### Date Filtering

Almost everything is interface-driven.  If you want to use date filtering, you just implement `IHasCreatedTimestamp` 
and/or `IHasModifiedTimestamp` (if you use both, you can use `DateFilter` to handle all the querying in one call).

It's usually best to use UTC for these timestamps, but that's not enforced.

`CreatedFrom` and `ModifiedFrom` filters are inclusive; `CreatedTo` and `ModifiedTo` are exclusive.

```csharp
// Entity Definition
public class MyEntity : IHasCreatedTimestamp, IHasModifiedTimestamp
{
    public DateTime DateCreated { get; } = DateTimeOffsetProvider.UtcNow.UtcDateTime;
    public DateTime DateModified { get; private set; } = DateTimeOffsetProvider.UtcNow.DateTime;
}

// Data-Layer (EF for example purposes)
public class MyEntityRepository(MyDbContext context)
{
    // Using the single date filter helper
    public async Task<IReadOnlyList<MyEntity>> GetAsync(
        DateFilter? dateFilter = null, 
        CancellationToken cancellationToken = default) =>
        await context
            .ApplyDateFilter(dateFilter)
            .ToListAsync(cancellationToken);
    
    // Applying each filter individually
    public async Task<IReadOnlyCollection<MyEntity>> GetAsync(
        DateFilter? dateFilter = null, 
        CancellationToken cancellationToken = default) =>
        await context
            .ApplyCreatedFromFilter(dateFilter?.CreatedFrom)
            .ApplyCreatedToFilter(dateFilter?.CreatedTo)
            .ApplyModifiedFromFilter(dateFilter?.ModifiedFrom)
            .ApplyModifiedToFilter(dateFilter?.ModifiedTo)
            .ToListAsync(cancellationToken);
}
```

### Cursor-Based Paging

If you want to use cursor-based paging, you implement `IHasCursor` and use the `ApplyCursorPagination` query filter 
helper.  Pagination should always be the last step prior to materialising the result.

```csharp
// Entity Definition
public class MyEntity : IHasCursor, IHasCreatedTimestamp, IHasModifiedTimestamp
{
    public long Cursor { get; }
    
    // ...
}

// Data-Layer (EF for example purposes)
public class MyEntityRepository(MyDbContext context) : IMyEntityRepository
{
    private const int DefaultPageSize = 100;
    
    public async Task<IReadOnlyCollection<MyEntity>> GetAsync(
        DateFilter? dateFilter = null,
        long? cursor = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default) =>
        await context
            .ApplyDateFilter(dateFilter)
            .ApplyCursorPagination(cursor, pageSize ?? DefaultPageSize, SortOrder.Ascending)
            .ToListAsync(cancellationToken);
}
```

If you want to use the `Beans.Pageable.Page` object, you can do so using the ToPage extension method.  

This returns the cursor of the last/first item in the page (for sort order ascending/descending, respectively) as an 
encoded string. This can be decoded using `TryDecodeCursor` and passed into the next request to get the next page.  
Providing no cursor (or a null value) will always return the first page.  

> [!Note]
> We use the "last returned cursor" for each page and an exclusive filter to avoid the need to "peek" at data beyond the
> scope of the current request.

```csharp
public class MyRequestHandler(IMyEntityRepository repository) 
    : IRequestHandler<MyRequest, Page<MyEntityDto>>
{
    public async Task<Page<MyEntityDto>> HandleAsync(MyRequest request, CancellationToken cancellationToken)
    {
        var data = await repository.GetAsync(
            new DateFilter(
                CreatedFrom: request.CreatedFrom,
                CreatedTo: request.CreatedTo,
                ModifiedFrom: request.ModifiedFrom,
                ModifiedTo: request.ModifiedTo),
            request.Cursor.TryDecodeCursor(out var cursor) ? cursor : null, 
            request.PageSize, 
            cancellationToken);
        return data.ToPage().MapTo<MyEntityDto>(item => new MyEntityDto(item));
    }
}
```

#### Generating Cursors

Cursors can be fussy, as they need to be unique to remain deterministic - any collisions introduce the risk that calling 
subsequent pages would skip records with the same cursor that didn't make it on to the page.

The cursor generation strategy is one that should be evaluated on a per-project basis based on things like:
- how many processes are you likely to run in parallel
- is it a read-heavy or a write-heavy application
- is object creation expected to be done in batches or one-by-one across a short period of time.  

It's very easy to overengineer this (as with anything involving the word "unique").  For small and early-stage projects, 
I tend to use the object creation timestamp at microsecond granularity and a unique index in the database to enforce it.

This strategy is implemented by `Beans.Pageable.CursorGenerator`, with a little single-process protection ensuring that 
no two objects in the same process can be given the same cursor.  This also uses the `DateTimeOffsetProvider` from 
`Beans.Common`, so it can be fixed for testing purposes.

```csharp
public class MyEntity : IHasCursor
{
    public long Cursor { get; } = CursorGenerator.Next();
}
```

### Limit-Offset Paging

Cursor-based paging isn't the best fit for every use-case - when using traditional limit/offset paging, you can use the 
`ApplyOffsetPagination` query helper.  Pagination should always be the last step prior to materialising the result.

```csharp
// Entity Definition
public class MyEntity
{
    public long SomeSortableColumn { get; }
}

// Data-Layer (EF for example purposes)
public class MyEntityRepository(MyDbContext context) : IMyEntityRepository
{
    private const int DefaultPageSize = 100;
    
    public async Task<IReadOnlyCollection<MyEntity>> GetAsync(
        int? offset = null,
        int? limit = null,
        CancellationToken cancellationToken = default) =>
        await context
            .ApplyOffsetPagination(
                offset, 
                limit ?? DefaultPageSize, 
                queryable => queryable.OrderBy(item => item.SomeSortableColumn))
            .ToListAsync(cancellationToken);
}
```