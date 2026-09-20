namespace Beans.Pageable;

/// <summary>Describes an entity which is compatible with cursor-based pagination.</summary>
public interface IHasCursor
{
    /// <summary>The entity cursor value.</summary>
    long Cursor { get; }
}