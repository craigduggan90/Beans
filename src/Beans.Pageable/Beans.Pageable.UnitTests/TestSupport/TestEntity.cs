namespace Beans.Pageable.UnitTests.TestSupport;

internal sealed class TestEntity : IHasCreatedTimestamp, IHasModifiedTimestamp
{
    public DateTime DateCreated { get; init; }

    public DateTime DateModified { get; init; }

    public static TestEntity Created(DateTime created) => new() { DateCreated = created };

    public static TestEntity Modified(DateTime modified) => new() { DateModified = modified };

    public static TestEntity CreatedAndModified(DateTime created, DateTime modified)
        => new() { DateCreated = created, DateModified = modified };
}