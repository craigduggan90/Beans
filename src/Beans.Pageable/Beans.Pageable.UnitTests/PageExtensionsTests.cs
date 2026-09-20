using Beans.Pageable.UnitTests.TestSupport;

namespace Beans.Pageable.UnitTests;

public static class PageExtensionsTests
{
    private static List<CursorEntity> Entities(params long[] cursors)
        => cursors.Select(cursor => new CursorEntity(cursor)).ToList();

    public class ToPage
    {
        [Fact]
        public void ShouldReturnTheEntitiesAndTheirCount()
        {
            var entities = Entities(1, 2, 3);

            var actual = entities.ToPage();

            Assert.Equal(entities, actual.Data);
            Assert.Equal(3, actual.Count);
        }

        [Fact]
        public void ShouldReturnTheHighestCursor_WhenSortOrderIsAscending()
        {
            var actual = Entities(4, 9, 12).ToPage(SortOrder.Ascending);

            Assert.True(actual.Cursor.TryDecodeCursor(out var cursor));
            Assert.Equal(12, cursor);
        }

        [Fact]
        public void ShouldReturnTheHighestCursor_WhenAscendingAndEntitiesAreOutOfOrder()
        {
            var actual = Entities(12, 4, 9).ToPage(SortOrder.Ascending);

            Assert.True(actual.Cursor.TryDecodeCursor(out var cursor));
            Assert.Equal(12, cursor);
        }

        [Fact]
        public void ShouldReturnTheLowestCursor_WhenSortOrderIsDescending()
        {
            var actual = Entities(12, 9, 4).ToPage(SortOrder.Descending);

            Assert.True(actual.Cursor.TryDecodeCursor(out var cursor));
            Assert.Equal(4, cursor);
        }

        [Fact]
        public void ShouldReturnTheLowestCursor_WhenDescendingAndEntitiesAreOutOfOrder()
        {
            var actual = Entities(9, 4, 12).ToPage(SortOrder.Descending);

            Assert.True(actual.Cursor.TryDecodeCursor(out var cursor));
            Assert.Equal(4, cursor);
        }

        [Fact]
        public void ShouldBeAscending_WhenSortOrderIsNotSpecified()
        {
            var actual = Entities(1, 2, 3).ToPage();

            Assert.Equal(SortOrder.Ascending, actual.SortOrder);
            Assert.True(actual.Cursor.TryDecodeCursor(out var cursor));
            Assert.Equal(3, cursor);
        }

        [Theory]
        [InlineData(SortOrder.Ascending)]
        [InlineData(SortOrder.Descending)]
        public void ShouldReturnTheSortOrder(SortOrder sortOrder)
        {
            var actual = Entities(1, 2, 3).ToPage(sortOrder);

            Assert.Equal(sortOrder, actual.SortOrder);
        }

        [Theory]
        [InlineData(SortOrder.Ascending)]
        [InlineData(SortOrder.Descending)]
        public void ShouldReturnNoCursorAndZeroCount_WhenThereAreNoEntities(SortOrder sortOrder)
        {
            var actual = Entities().ToPage(sortOrder);

            Assert.Empty(actual.Data);
            Assert.Null(actual.Cursor);
            Assert.Equal(0, actual.Count);
        }

        [Theory]
        [InlineData(SortOrder.Ascending)]
        [InlineData(SortOrder.Descending)]
        public void ShouldReturnTheOnlyCursor_WhenThereIsOneEntity(SortOrder sortOrder)
        {
            var actual = Entities(7).ToPage(sortOrder);

            Assert.True(actual.Cursor.TryDecodeCursor(out var cursor));
            Assert.Equal(7, cursor);
        }
    }
}