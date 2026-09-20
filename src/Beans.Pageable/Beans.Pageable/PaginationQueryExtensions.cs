namespace Beans.Pageable;

/// <summary>Extension methods for paginating queries.</summary>
public static class PaginationQueryExtensions
{
    /// <summary>Apply limit/offset pagination to a collection of objects.</summary>
    /// <param name="queryable">The collection of objects to paginate.</param>
    /// <param name="offset">The number of items to skip.</param>
    /// <param name="limit">The number of items to return.</param>
    /// <param name="sortFunction">An optional operation to order the objects prior to pagination.</param>
    /// <typeparam name="T">The type of object in the collection.</typeparam>
    /// <returns>The page of items in a collection.</returns>
    public static IQueryable<T> ApplyOffsetPagination<T>(
        this IQueryable<T> queryable,
        int? offset,
        int limit,
        Func<IQueryable<T>, IOrderedQueryable<T>> sortFunction) =>
        sortFunction(queryable).Skip(offset ?? 0).Take(limit);

    /// <summary>Apply cursor based pagination to a collection of objects which implement <see cref="IHasCursor"/>.</summary>
    /// <param name="queryable">The collection of objects to paginate.</param>
    /// <param name="cursor">The cursor of the earliest/latest item on the previous page (depending on sortOrder).</param>
    /// <param name="pageSize">The number of items to return.</param>
    /// <param name="sortOrder">The order in which the items are sorted.</param>
    /// <typeparam name="T">The type of object in the collection.</typeparam>
    /// <returns>The page of items in a collection.</returns>
    public static IQueryable<T> ApplyCursorPagination<T>(
        this IQueryable<T> queryable,
        long? cursor,
        int pageSize,
        SortOrder sortOrder = SortOrder.Ascending)
        where T : IHasCursor =>
        sortOrder == SortOrder.Descending
            ? queryable.ApplyCursorPaginationDescending(cursor, pageSize)
            : queryable.ApplyCursorPaginationAscending(cursor, pageSize);

    private static IQueryable<T> ApplyCursorPaginationAscending<T>(
        this IQueryable<T> queryable,
        long? cursor,
        int pageSize)
        where T : IHasCursor
    {
        queryable = cursor.HasValue ? queryable.Where(item => item.Cursor > cursor) : queryable;
        return queryable.OrderBy(item => item.Cursor).Take(pageSize);
    }

    private static IQueryable<T> ApplyCursorPaginationDescending<T>(
        this IQueryable<T> queryable,
        long? cursor,
        int pageSize)
        where T : IHasCursor
    {
        queryable = cursor.HasValue ? queryable.Where(item => item.Cursor < cursor) : queryable;
        return queryable.OrderByDescending(item => item.Cursor).Take(pageSize);
    }
}