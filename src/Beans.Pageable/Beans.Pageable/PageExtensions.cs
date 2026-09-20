namespace Beans.Pageable;

/// <summary>Extension methods associated Page objects.</summary>
public static class PageExtensions
{
    /// <summary>Converts a collection of objects to a <see cref="Page{T}"/>.</summary>
    /// <param name="collection">The objects to represent in the cursor page.</param>
    /// <param name="sortOrder">
    /// The order in which objects were sorted for pagination - used to determine whether the first or last item should
    /// be used in the Cursor property.
    /// </param>
    /// <typeparam name="T">The type of object in the collection.</typeparam>
    /// <returns>A <see cref="Page{T}"/> representing the provided collection.</returns>
    public static Page<T> ToPage<T>(
        this IReadOnlyCollection<T> collection,
        SortOrder sortOrder = SortOrder.Ascending)
        where T : IHasCursor
        => new(
            Data: collection,
            Cursor: sortOrder == SortOrder.Descending
                ? collection.GetEarliestCursor()
                : collection.GetLatestCursor(),
            Count: collection.Count,
            SortOrder: sortOrder);

    private static string? GetLatestCursor<T>(this IReadOnlyCollection<T> pageData)
        where T : IHasCursor =>
        pageData is { Count: > 0 }
            ? pageData.MaxBy(e => e.Cursor)?.Cursor.EncodeCursor()
            : null;

    private static string? GetEarliestCursor<T>(this IReadOnlyCollection<T> pageData)
        where T : IHasCursor =>
        pageData is { Count: > 0 }
            ? pageData.MinBy(e => e.Cursor)?.Cursor.EncodeCursor()
            : null;
}