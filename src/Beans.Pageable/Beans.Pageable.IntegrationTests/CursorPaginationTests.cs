using Beans.Common.Providers;
using Beans.Pageable.IntegrationTests.TestSupport;
using Microsoft.EntityFrameworkCore;

namespace Beans.Pageable.IntegrationTests;

public static class CursorPaginationTests
{
    public class ApplyCursorPagination : DatabaseTestBase
    {
        private async Task<List<Widget>> SeedAsync(int count)
        {
            var widgets = Enumerable.Range(1, count).Select(i => new Widget { Name = $"{i}" }).ToList();
            await SeedAsync(widgets.ToArray());
            return widgets;
        }

        private async Task<string[]> NamesAsync(long? cursor, int pageSize, SortOrder sortOrder = SortOrder.Ascending)
        {
            await using var context = NewContext();
            var widgets = await context.Widgets
                .ApplyCursorPagination(cursor, pageSize, sortOrder)
                .ToListAsync(TestContext.Current.CancellationToken);

            return widgets.Select(widget => widget.Name).ToArray();
        }

        [Fact]
        public async Task ShouldReturnTheFirstPageOldestFirst_WhenCursorIsNull()
        {
            await SeedAsync(5);

            Assert.Equal(["1", "2", "3"], await NamesAsync(null, 3));
        }

        [Fact]
        public async Task ShouldReturnOnlyItemsAfterTheCursor_WhenAscending()
        {
            var widgets = await SeedAsync(5);

            Assert.Equal(["3", "4", "5"], await NamesAsync(widgets[1].Cursor, 10));
        }

        [Fact]
        public async Task ShouldExcludeTheEntityAtTheCursor_WhenAscending()
        {
            var widgets = await SeedAsync(5);

            Assert.Empty(await NamesAsync(widgets[^1].Cursor, 10));
        }

        [Fact]
        public async Task ShouldReturnTheFirstPageNewestFirst_WhenDescendingAndCursorIsNull()
        {
            await SeedAsync(5);

            Assert.Equal(["5", "4", "3"], await NamesAsync(null, 3, SortOrder.Descending));
        }

        [Fact]
        public async Task ShouldReturnOnlyItemsBeforeTheCursor_WhenDescending()
        {
            var widgets = await SeedAsync(5);

            Assert.Equal(["3", "2", "1"], await NamesAsync(widgets[3].Cursor, 10, SortOrder.Descending));
        }

        [Theory]
        [InlineData(SortOrder.Ascending, new[] { "1", "2", "3", "4", "5", "6", "7" })]
        [InlineData(SortOrder.Descending, new[] { "7", "6", "5", "4", "3", "2", "1" })]
        public async Task ShouldReturnEveryEntityExactlyOnce_WhenPagingThroughUsingEachPagesCursor(
            SortOrder sortOrder,
            string[] expected)
        {
            await SeedAsync(7);
            var seen = new List<string>();
            string? cursor = null;

            for (var attempt = 0; attempt < 10; attempt++)
            {
                Assert.True(cursor.TryDecodeCursor(out var decoded));

                await using var context = NewContext();
                var page = (await context.Widgets
                        .ApplyCursorPagination(decoded, 3, sortOrder)
                        .ToListAsync(TestContext.Current.CancellationToken))
                    .ToPage(sortOrder);

                if (page.Count == 0)
                    break;

                seen.AddRange(page.Data.Select(widget => widget.Name));
                cursor = page.Cursor;
            }

            Assert.Equal(expected, seen);
        }
    }

    public class GeneratedCursors : DatabaseTestBase
    {
        [Fact]
        public async Task ShouldKeepTheStoredCursorsAndDates_WhenEntitiesAreReloaded()
        {
            var widgets = Enumerable.Range(1, 5).Select(i => new Widget { Name = $"{i}" }).ToArray();
            await SeedAsync(widgets);

            await using var context = NewContext();
            var reloaded = await context.Widgets.OrderBy(widget => widget.Id)
                .ToListAsync(TestContext.Current.CancellationToken);

            Assert.Equal(widgets.Select(w => w.Cursor), reloaded.Select(w => w.Cursor));
            Assert.Equal(widgets.Select(w => w.DateCreated), reloaded.Select(w => w.DateCreated));
            Assert.All(reloaded, widget => Assert.Equal(DateTimeKind.Utc, widget.DateCreated.Kind));
        }

        [Fact]
        public async Task ShouldStoreDistinctIncreasingCursors_WhenWidgetsAreCreatedAtTheSameTime()
        {
            Widget[] widgets;
            using (new DateTimeOffsetProviderContext(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)))
                widgets = Enumerable.Range(1, 25).Select(i => new Widget { Name = $"{i}" }).ToArray();

            // The cursor column has a unique index, so a repeated cursor would fail here.
            await SeedAsync(widgets);

            await using var context = NewContext();
            var cursors = await context.Widgets.OrderBy(widget => widget.Id)
                .Select(widget => widget.Cursor)
                .ToListAsync(TestContext.Current.CancellationToken);

            Assert.Equal(25, cursors.Distinct().Count());
            Assert.Equal(cursors.Order(), cursors);
        }
    }
}