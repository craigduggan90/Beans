using Beans.Pageable.UnitTests.TestSupport;

namespace Beans.Pageable.UnitTests;

public static class PaginationQueryExtensionsTests
{
    private static IQueryable<CursorEntity> Entities(params long[] cursors)
        => cursors.Select(cursor => new CursorEntity(cursor)).AsQueryable();

    private static IOrderedQueryable<CursorEntity> ByCursor(IQueryable<CursorEntity> queryable)
        => queryable.OrderBy(entity => entity.Cursor);

    private static long[] Cursors(IEnumerable<CursorEntity> entities) => entities.Select(e => e.Cursor).ToArray();

    public class ApplyOffsetPagination
    {
        [Fact]
        public void ShouldSkipTheOffsetAndTakeTheLimit()
        {
            var actual = Entities(1, 2, 3, 4, 5, 6).ApplyOffsetPagination(2, 3, ByCursor);

            Assert.Equal([3L, 4L, 5L], Cursors(actual));
        }

        [Fact]
        public void ShouldNotSkipAnything_WhenOffsetIsNull()
        {
            var actual = Entities(1, 2, 3, 4).ApplyOffsetPagination(null, 2, ByCursor);

            Assert.Equal([1L, 2L], Cursors(actual));
        }

        [Fact]
        public void ShouldReturnFewerThanTheLimit_WhenNotEnoughItemsRemain()
        {
            var actual = Entities(1, 2, 3, 4).ApplyOffsetPagination(3, 5, ByCursor);

            Assert.Equal([4L], Cursors(actual));
        }

        [Fact]
        public void ShouldReturnNothing_WhenTheOffsetIsBeyondTheEnd()
        {
            var actual = Entities(1, 2, 3).ApplyOffsetPagination(10, 5, ByCursor);

            Assert.Empty(actual);
        }

        [Fact]
        public void ShouldApplyTheSortFunctionBeforeSkippingAndTaking()
        {
            var actual = Entities(1, 2, 3, 4, 5)
                .ApplyOffsetPagination(1, 2, q => q.OrderByDescending(entity => entity.Cursor));

            Assert.Equal([4L, 3L], Cursors(actual));
        }

        [Fact]
        public void ShouldSupportASortFunctionWithMoreThanOneSortKey()
        {
            // Even cursors first, then odd, each group ascending: 2, 4, 6, 1, 3, 5
            var actual = Entities(1, 2, 3, 4, 5, 6).ApplyOffsetPagination(
                1,
                3,
                q => q.OrderBy(entity => entity.Cursor % 2).ThenBy(entity => entity.Cursor));

            Assert.Equal([4L, 6L, 1L], Cursors(actual));
        }

        [Fact]
        public void ShouldReturnEveryItemExactlyOnce_WhenPagingThroughWithIncreasingOffsets()
        {
            var entities = Entities(5, 3, 7, 1, 6, 2, 4);
            var seen = new List<long>();

            for (var offset = 0; offset < 10; offset += 3)
                seen.AddRange(Cursors(entities.ApplyOffsetPagination(offset, 3, ByCursor)));

            Assert.Equal([1L, 2L, 3L, 4L, 5L, 6L, 7L], seen);
        }
    }

    public class ApplyCursorPagination
    {
        [Fact]
        public void ShouldReturnTheFirstPageInAscendingOrder_WhenCursorIsNull()
        {
            var actual = Entities(3, 1, 5, 2, 4).ApplyCursorPagination(null, 3);

            Assert.Equal([1L, 2L, 3L], Cursors(actual));
        }

        [Fact]
        public void ShouldReturnOnlyItemsAfterTheCursor_WhenAscending()
        {
            var actual = Entities(1, 2, 3, 4, 5).ApplyCursorPagination(2, 10, SortOrder.Ascending);

            Assert.Equal([3L, 4L, 5L], Cursors(actual));
        }

        [Fact]
        public void ShouldExcludeTheItemAtTheCursor_WhenAscending()
        {
            var actual = Entities(1, 2, 3).ApplyCursorPagination(3, 10, SortOrder.Ascending);

            Assert.Empty(actual);
        }

        [Fact]
        public void ShouldKeepEverything_WhenTheCursorIsBeforeTheFirstItemAndAscending()
        {
            var actual = Entities(5, 6).ApplyCursorPagination(0, 10, SortOrder.Ascending);

            Assert.Equal([5L, 6L], Cursors(actual));
        }

        [Fact]
        public void ShouldTakeOnlyThePageSize_WhenAscending()
        {
            var actual = Entities(1, 2, 3, 4, 5).ApplyCursorPagination(1, 2, SortOrder.Ascending);

            Assert.Equal([2L, 3L], Cursors(actual));
        }

        [Fact]
        public void ShouldBeAscending_WhenSortOrderIsNotSpecified()
        {
            var actual = Entities(3, 1, 2).ApplyCursorPagination(null, 10);

            Assert.Equal([1L, 2L, 3L], Cursors(actual));
        }

        [Fact]
        public void ShouldReturnTheFirstPageInDescendingOrder_WhenCursorIsNull()
        {
            var actual = Entities(3, 1, 5, 2, 4).ApplyCursorPagination(null, 3, SortOrder.Descending);

            Assert.Equal([5L, 4L, 3L], Cursors(actual));
        }

        [Fact]
        public void ShouldReturnOnlyItemsBeforeTheCursor_WhenDescending()
        {
            var actual = Entities(1, 2, 3, 4, 5).ApplyCursorPagination(4, 10, SortOrder.Descending);

            Assert.Equal([3L, 2L, 1L], Cursors(actual));
        }

        [Fact]
        public void ShouldExcludeTheItemAtTheCursor_WhenDescending()
        {
            var actual = Entities(1, 2, 3).ApplyCursorPagination(1, 10, SortOrder.Descending);

            Assert.Empty(actual);
        }

        [Fact]
        public void ShouldKeepEverything_WhenTheCursorIsAfterTheLastItemAndDescending()
        {
            var actual = Entities(5, 6).ApplyCursorPagination(100, 10, SortOrder.Descending);

            Assert.Equal([6L, 5L], Cursors(actual));
        }

        [Fact]
        public void ShouldTakeOnlyThePageSize_WhenDescending()
        {
            var actual = Entities(1, 2, 3, 4, 5).ApplyCursorPagination(5, 2, SortOrder.Descending);

            Assert.Equal([4L, 3L], Cursors(actual));
        }

        [Theory]
        [InlineData(SortOrder.Ascending, new long[] { 1, 2, 3, 4, 5, 6, 7 })]
        [InlineData(SortOrder.Descending, new long[] { 7, 6, 5, 4, 3, 2, 1 })]
        public void ShouldReturnEveryItemExactlyOnce_WhenPagingThroughUsingEachPagesCursor(
            SortOrder sortOrder,
            long[] expected)
        {
            var entities = Entities(5, 3, 7, 1, 6, 2, 4);
            var seen = new List<long>();
            string? cursor = null;

            for (var attempt = 0; attempt < 10; attempt++)
            {
                Assert.True(cursor.TryDecodeCursor(out var decoded));
                var page = entities.ApplyCursorPagination(decoded, 3, sortOrder).ToList().ToPage(sortOrder);
                if (page.Count == 0)
                    break;

                seen.AddRange(Cursors(page.Data));
                cursor = page.Cursor;
            }

            Assert.Equal(expected, seen);
        }
    }
}