using Microsoft.EntityFrameworkCore;

namespace Beans.Pageable.IntegrationTests.TestSupport;

public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public DbSet<Widget> Widgets => Set<Widget>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Widget>(builder =>
        {
            builder.HasKey(widget => widget.Id);

            // Cursor and DateCreated are get-only, so EF needs to be told to map them.
            builder.Property(widget => widget.Cursor);
            builder.HasIndex(widget => widget.Cursor).IsUnique();

            builder.Property(widget => widget.DateCreated).HasConversion<UtcDateTimeConverter>();
            builder.Property(widget => widget.DateModified).HasConversion<UtcDateTimeConverter>();
            builder.HasIndex(widget => widget.DateCreated);
            builder.HasIndex(widget => widget.DateModified);
        });
}