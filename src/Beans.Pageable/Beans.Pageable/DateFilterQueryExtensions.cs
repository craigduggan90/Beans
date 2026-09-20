namespace Beans.Pageable;

/// <summary>
/// Extension methods for filtering queries of entities implementing <see cref="IHasCreatedTimestamp"/> and
/// <see cref="IHasModifiedTimestamp"/> by date.
/// </summary>
public static class DateFilterQueryExtensions
{
    /// <summary>Applies every date in a <see cref="DateFilter"/> to the collection.</summary>
    /// <param name="queryable">The collection to filter.</param>
    /// <param name="value">The filter to apply.  When <see langword="null"/>, the collection is not filtered.</param>
    /// <typeparam name="T">The implementation of the timestamp interfaces in the queryable.</typeparam>
    /// <returns>A reference to the queryable after the filter operation.</returns>
    public static IQueryable<T> ApplyDateFilter<T>(this IQueryable<T> queryable, DateFilter? value)
        where T : IHasCreatedTimestamp, IHasModifiedTimestamp
        => value == null
            ? queryable
            : queryable
                .ApplyCreatedFromFilter(value.CreatedFrom)
                .ApplyCreatedToFilter(value.CreatedTo)
                .ApplyModifiedFromFilter(value.ModifiedFrom)
                .ApplyModifiedToFilter(value.ModifiedTo);

    /// <summary>Filters a collection of <typeparamref name="T"/> objects by minimum creation date (inclusive).</summary>
    /// <param name="queryable">The collection to filter.</param>
    /// <param name="value">The value to filter by.</param>
    /// <typeparam name="T">The implementation of <see cref="IHasCreatedTimestamp"/> in the queryable.</typeparam>
    /// <returns>A reference to the queryable after the filter operation.</returns>
    public static IQueryable<T> ApplyCreatedFromFilter<T>(this IQueryable<T> queryable, DateTime? value)
        where T : IHasCreatedTimestamp
        => value == null
            ? queryable
            : queryable.Where(instance => instance.DateCreated >= value.Value);

    /// <summary>Filters a collection of <typeparamref name="T"/> objects by maximum creation date (exclusive).</summary>
    /// <param name="queryable">The collection to filter.</param>
    /// <param name="value">The value to filter by.</param>
    /// <typeparam name="T">The implementation of <see cref="IHasCreatedTimestamp"/> in the queryable.</typeparam>
    /// <returns>A reference to the queryable after the filter operation.</returns>
    public static IQueryable<T> ApplyCreatedToFilter<T>(this IQueryable<T> queryable, DateTime? value)
        where T : IHasCreatedTimestamp
        => value == null
            ? queryable
            : queryable.Where(instance => instance.DateCreated < value.Value);

    /// <summary>
    /// Filters a collection of <typeparamref name="T"/> objects by minimum modification date (inclusive).
    /// </summary>
    /// <param name="queryable">The collection to filter.</param>
    /// <param name="value">The value to filter by.</param>
    /// <typeparam name="T">The implementation of <see cref="IHasModifiedTimestamp"/> in the queryable.</typeparam>
    /// <returns>A reference to the queryable after the filter operation.</returns>
    public static IQueryable<T> ApplyModifiedFromFilter<T>(this IQueryable<T> queryable, DateTime? value)
        where T : IHasModifiedTimestamp
        => value == null
            ? queryable
            : queryable.Where(instance => instance.DateModified >= value.Value);

    /// <summary>
    /// Filters a collection of <typeparamref name="T"/> objects by maximum modification date (exclusive).
    /// </summary>
    /// <param name="queryable">The collection to filter.</param>
    /// <param name="value">The value to filter by.</param>
    /// <typeparam name="T">The implementation of <see cref="IHasModifiedTimestamp"/> in the queryable.</typeparam>
    /// <returns>A reference to the queryable after the filter operation.</returns>
    public static IQueryable<T> ApplyModifiedToFilter<T>(this IQueryable<T> queryable, DateTime? value)
        where T : IHasModifiedTimestamp
        => value == null
            ? queryable
            : queryable.Where(instance => instance.DateModified < value.Value);
}