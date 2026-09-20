namespace Beans.Pageable;

/// <summary>Describes an entity which includes the timestamp of its last modification.</summary>
public interface IHasModifiedTimestamp
{
    /// <summary>The entity modification timestamp.</summary>
    DateTime DateModified { get; }
}