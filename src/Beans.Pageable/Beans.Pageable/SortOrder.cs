namespace Beans.Pageable;

/// <summary> Enum describing the way in which <see cref="IHasCursor"/> objects are sorted for pagination.</summary>
public enum SortOrder
{
    /// <summary>
    /// Items are sorted in ascending order (oldest to newest).
    /// </summary>
    Ascending,

    /// <summary>
    /// Items are sorted in descending order (newest to oldest)
    /// </summary>
    Descending
}