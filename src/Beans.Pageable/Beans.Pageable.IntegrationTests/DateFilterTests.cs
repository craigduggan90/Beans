using Beans.Common.Providers;
using Beans.Pageable.IntegrationTests.TestSupport;
using Microsoft.EntityFrameworkCore;

namespace Beans.Pageable.IntegrationTests;

public static class DateFilterTests
{
    private static DateTime Day(int month, int day) => new(2026, month, day, 0, 0, 0, DateTimeKind.Utc);

    private static DateTimeOffset At(int month, int day) => new(Day(month, day), TimeSpan.Zero);

    public class ApplyDateFilter : DatabaseTestBase
    {
        // Widgets "Jan1".."Jan5", each created on that day of January, and modified on that day of February.
        private async Task SeedAsync()
        {
            var widgets = Enumerable.Range(1, 5).Select(day =>
            {
                var widget = CreateWidget($"Jan{day}", At(1, day));
                using var _ = new DateTimeOffsetProviderContext(At(2, day));
                widget.Touch();
                return widget;
            });

            await SeedAsync(widgets.ToArray());
        }

        private async Task<string[]> NamesAsync(DateFilter? filter)
        {
            await using var context = NewContext();
            var widgets = await context.Widgets
                .ApplyDateFilter(filter)
                .OrderBy(widget => widget.Name)
                .ToListAsync(TestContext.Current.CancellationToken);

            return widgets.Select(widget => widget.Name).ToArray();
        }

        [Fact]
        public async Task ShouldReturnEverything_WhenTheFilterIsNull()
        {
            await SeedAsync();

            Assert.Equal(["Jan1", "Jan2", "Jan3", "Jan4", "Jan5"], await NamesAsync(null));
        }

        [Fact]
        public async Task ShouldReturnEverything_WhenTheFilterHasNoDates()
        {
            await SeedAsync();

            Assert.Equal(["Jan1", "Jan2", "Jan3", "Jan4", "Jan5"], await NamesAsync(new DateFilter()));
        }

        [Fact]
        public async Task ShouldIncludeTheFromDateAndExcludeTheToDate_WhenFilteringByCreated()
        {
            await SeedAsync();

            var actual = await NamesAsync(new DateFilter(CreatedFrom: Day(1, 2), CreatedTo: Day(1, 4)));

            Assert.Equal(["Jan2", "Jan3"], actual);
        }

        [Fact]
        public async Task ShouldIncludeTheFromDateAndExcludeTheToDate_WhenFilteringByModified()
        {
            await SeedAsync();

            var actual = await NamesAsync(new DateFilter(ModifiedFrom: Day(2, 2), ModifiedTo: Day(2, 4)));

            Assert.Equal(["Jan2", "Jan3"], actual);
        }

        [Fact]
        public async Task ShouldOnlyReturnEntitiesInBothRanges_WhenCreatedAndModifiedAreFiltered()
        {
            await SeedAsync();

            var actual = await NamesAsync(new DateFilter(
                CreatedFrom: Day(1, 2),
                CreatedTo: Day(1, 5),
                ModifiedFrom: Day(2, 3)));

            Assert.Equal(["Jan3", "Jan4"], actual);
        }

        [Fact]
        public async Task ShouldApplyEachFilterIndependently_WhenUsingTheIndividualHelpers()
        {
            await SeedAsync();
            await using var context = NewContext();

            var createdFrom = await context.Widgets.ApplyCreatedFromFilter(Day(1, 4)).CountAsync(TestContext.Current.CancellationToken);
            var createdTo = await context.Widgets.ApplyCreatedToFilter(Day(1, 2)).CountAsync(TestContext.Current.CancellationToken);
            var modifiedFrom = await context.Widgets.ApplyModifiedFromFilter(Day(2, 4)).CountAsync(TestContext.Current.CancellationToken);
            var modifiedTo = await context.Widgets.ApplyModifiedToFilter(Day(2, 2)).CountAsync(TestContext.Current.CancellationToken);

            Assert.Equal(2, createdFrom);   // Jan4, Jan5 (inclusive)
            Assert.Equal(1, createdTo);     // Jan1 (exclusive)
            Assert.Equal(2, modifiedFrom);  // Jan4, Jan5 (inclusive)
            Assert.Equal(1, modifiedTo);    // Jan1 (exclusive)
        }

        [Fact]
        public async Task ShouldPageThroughTheFilteredResults_WhenFollowedByCursorPagination()
        {
            await SeedAsync();
            var filter = new DateFilter(CreatedFrom: Day(1, 2), CreatedTo: Day(1, 6));   // Jan2..Jan5
            var seen = new List<string[]>();
            string? cursor = null;

            for (var attempt = 0; attempt < 5; attempt++)
            {
                Assert.True(cursor.TryDecodeCursor(out var decoded));

                await using var context = NewContext();
                var page = (await context.Widgets
                        .ApplyDateFilter(filter)
                        .ApplyCursorPagination(decoded, 2)
                        .ToListAsync(TestContext.Current.CancellationToken))
                    .ToPage();

                if (page.Count == 0)
                    break;

                seen.Add(page.Data.Select(widget => widget.Name).ToArray());
                cursor = page.Cursor;
            }

            Assert.Equal(2, seen.Count);
            Assert.Equal(["Jan2", "Jan3"], seen[0]);
            Assert.Equal(["Jan4", "Jan5"], seen[1]);
        }
    }
}