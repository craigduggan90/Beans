using Beans.Pageable.UnitTests.TestSupport;

namespace Beans.Pageable.UnitTests;

public static class PageTests
{
    public class MapTo
    {
        [Fact]
        public void ShouldReturnTheMappedItems()
        {
            var page = new Page<int>([1, 2, 3], "cursor", 3, SortOrder.Ascending);

            var actual = page.MapTo(number => $"item {number}");

            Assert.Equal(["item 1", "item 2", "item 3"], actual.Data);
        }

        [Theory]
        [InlineData(SortOrder.Ascending)]
        [InlineData(SortOrder.Descending)]
        public void ShouldKeepTheCursorCountAndSortOrder(SortOrder sortOrder)
        {
            var page = new Page<int>([1, 2, 3], "cursor", 3, sortOrder);

            var actual = page.MapTo(number => number * 2);

            Assert.Equal("cursor", actual.Cursor);
            Assert.Equal(3, actual.Count);
            Assert.Equal(sortOrder, actual.SortOrder);
        }

        [Fact]
        public void ShouldReturnAnEmptyPage_WhenThePageIsEmpty()
        {
            var page = new Page<int>([], null, 0, SortOrder.Ascending);

            var actual = page.MapTo(_ => "unused");

            Assert.Empty(actual.Data);
            Assert.Null(actual.Cursor);
            Assert.Equal(0, actual.Count);
        }

        [Fact]
        public void ShouldNotCallTheConverter_WhenThePageIsEmpty()
        {
            var called = false;

            new Page<int>([], null, 0, SortOrder.Ascending).MapTo(number =>
            {
                called = true;
                return number;
            });

            Assert.False(called);
        }

        [Fact]
        public void ShouldNotChangeTheOriginalPage()
        {
            var page = new Page<int>([1, 2], "cursor", 2, SortOrder.Ascending);

            page.MapTo(number => number * 10);

            Assert.Equal([1, 2], page.Data);
        }

        [Fact]
        public void ShouldKeepTheCursorOfTheEntities_WhenCalledOnAPageFromToPage()
        {
            var entities = new List<CursorEntity> { new(3), new(8) };

            var actual = entities.ToPage().MapTo(_ => "same");

            Assert.Equal(["same", "same"], actual.Data);
            Assert.True(actual.Cursor.TryDecodeCursor(out var cursor));
            Assert.Equal(8, cursor);
        }
    }
}