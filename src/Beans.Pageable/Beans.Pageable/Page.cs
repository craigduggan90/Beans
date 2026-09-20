namespace Beans.Pageable;

/// <summary>Represents a page of objects.</summary>
/// <param name="Data">The page of objects.</param>
/// <param name="Cursor">The cursor required to retrieve the next page of objects.</param>
/// <param name="Count">The number of objects in the page.</param>
/// <param name="SortOrder">The pagination order.</param>
/// <typeparam name="T">The type of object represented by the cursor page.</typeparam>
public record Page<T>(IReadOnlyCollection<T> Data, string? Cursor, int Count, SortOrder SortOrder)
{
    /// <summary>
    /// Creates a new <see cref="Page{T}"/> with all <typeparamref name="T"/> objects in <see cref="Data"/>
    /// converted to <typeparamref name="TResult"/>.  All other values are retained from the current object. 
    /// </summary>
    /// <param name="converter">The function used to convert objects from <typeparamref name="T"/> to <typeparamref name="TResult"/>.</param>
    /// <typeparam name="TResult">The type of object represented by the returned <see cref="Page{T}"/>.</typeparam>
    /// <returns>A new <see cref="Page{T}"/> containing the converted objects.</returns>
    public Page<TResult> MapTo<TResult>(Func<T, TResult> converter)
        => new([.. Data.Select(converter)], Cursor, Count, SortOrder);
}