namespace Beans.Pageable;

/// <summary>Describes an entity which includes a creation timestamp.</summary>
public interface IHasCreatedTimestamp
{
    /// <summary>The entity creation timestamp (UTC).</summary>
    DateTime DateCreated { get; }
}