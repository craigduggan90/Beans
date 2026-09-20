using Beans.Common.Providers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Beans.Pageable.IntegrationTests.TestSupport;

/// <summary>Gives each test its own empty in-memory SQLite database.</summary>
public abstract class DatabaseTestBase : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Filename=:memory:");
    private DbContextOptions<TestDbContext>? _options;

    /// <summary>The context used to seed data.</summary>
    protected TestDbContext Context { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        await _connection.OpenAsync(TestContext.Current.CancellationToken);
        _options = new DbContextOptionsBuilder<TestDbContext>().UseSqlite(_connection).Options;

        Context = new TestDbContext(_options);
        await Context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    /// <summary>Creates a context with its own change tracker over the same database, so reads come from the database.</summary>
    protected TestDbContext NewContext() => new(_options!);

    /// <summary>Creates a widget as if it were created at the given time.</summary>
    protected static Widget CreateWidget(string name, DateTimeOffset createdAt)
    {
        using var _ = new DateTimeOffsetProviderContext(createdAt);
        return new Widget { Name = name };
    }

    /// <summary>Saves widgets, and clears the change tracker so later queries hit the database.</summary>
    protected async Task SeedAsync(params Widget[] widgets)
    {
        Context.Widgets.AddRange(widgets);
        await Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        Context.ChangeTracker.Clear();
    }
}