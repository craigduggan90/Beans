using Beans.Pageable.IntegrationTests.TestSupport;
using Microsoft.EntityFrameworkCore;

namespace Beans.Pageable.IntegrationTests;

public static class OffsetPaginationTests
{
    public class ApplyOffsetPagination : DatabaseTestBase
    {
        private async Task SeedAsync() =>
            await SeedAsync("a", "b", "c", "d", "e", "f");

        private async Task SeedAsync(params string[] names) =>
            await SeedAsync(names.Select(name => new Widget { Name = name }).ToArray());

        private async Task<string[]> NamesAsync(
            int? offset,
            int limit,
            Func<IQueryable<Widget>, IOrderedQueryable<Widget>> sort)
        {
            await using var context = NewContext();
            var widgets = await context.Widgets
                .ApplyOffsetPagination(offset, limit, sort)
                .ToListAsync(TestContext.Current.CancellationToken);

            return widgets.Select(widget => widget.Name).ToArray();
        }

        [Fact]
        public async Task ShouldSkipTheOffsetAndTakeTheLimit()
        {
            await SeedAsync();

            Assert.Equal(["c", "d", "e"], await NamesAsync(2, 3, q => q.OrderBy(w => w.Name)));
        }

        [Fact]
        public async Task ShouldNotSkipAnything_WhenOffsetIsNull()
        {
            await SeedAsync();

            Assert.Equal(["a", "b"], await NamesAsync(null, 2, q => q.OrderBy(w => w.Name)));
        }

        [Fact]
        public async Task ShouldReturnFewerThanTheLimit_WhenNotEnoughItemsRemain()
        {
            await SeedAsync();

            Assert.Equal(["f"], await NamesAsync(5, 3, q => q.OrderBy(w => w.Name)));
        }

        [Fact]
        public async Task ShouldReturnNothing_WhenTheOffsetIsBeyondTheEnd()
        {
            await SeedAsync();

            Assert.Empty(await NamesAsync(10, 3, q => q.OrderBy(w => w.Name)));
        }

        [Fact]
        public async Task ShouldApplyTheSortFunctionBeforeSkippingAndTaking()
        {
            await SeedAsync();

            Assert.Equal(["e", "d"], await NamesAsync(1, 2, q => q.OrderByDescending(w => w.Name)));
        }

        [Fact]
        public async Task ShouldReturnEveryEntityExactlyOnce_WhenPagingThroughWithIncreasingOffsets()
        {
            await SeedAsync();
            var seen = new List<string>();

            for (var offset = 0; offset < 8; offset += 2)
                seen.AddRange(await NamesAsync(offset, 2, q => q.OrderBy(w => w.Name)));

            Assert.Equal(["a", "b", "c", "d", "e", "f"], seen);
        }
    }
}