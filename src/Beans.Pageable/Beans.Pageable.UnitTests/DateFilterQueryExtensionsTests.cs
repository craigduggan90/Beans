using Beans.Pageable.UnitTests.TestSupport;

namespace Beans.Pageable.UnitTests;

public static class DateFilterQueryExtensionsTests
{
    private static readonly DateTime Jan1 = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Jan2 = Jan1.AddDays(1);
    private static readonly DateTime Jan3 = Jan1.AddDays(2);

    private static IQueryable<TestEntity> CreatedEntities()
        => new[] { Jan1, Jan2, Jan3 }.Select(TestEntity.Created).AsQueryable();

    private static IQueryable<TestEntity> ModifiedEntities()
        => new[] { Jan1, Jan2, Jan3 }.Select(TestEntity.Modified).AsQueryable();

    // Entity N was created on day N, and modified on day N + 10.
    private static IQueryable<TestEntity> Entities()
        => Enumerable.Range(1, 5)
            .Select(day => TestEntity.CreatedAndModified(Jan1.AddDays(day), Jan1.AddDays(day + 10)))
            .AsQueryable();

    private static DateTime Day(int day) => Jan1.AddDays(day);

    private static int[] CreatedDays(IQueryable<TestEntity> entities)
        => entities.Select(e => (e.DateCreated - Jan1).Days).ToArray();

    public class ApplyDateFilter
    {
        [Fact]
        public void ShouldNotFilter_WhenFilterIsNull()
        {
            var actual = Entities().ApplyDateFilter(null);

            Assert.Equal([1, 2, 3, 4, 5], CreatedDays(actual));
        }

        [Fact]
        public void ShouldNotFilter_WhenFilterHasNoValues()
        {
            var actual = Entities().ApplyDateFilter(new DateFilter());

            Assert.Equal([1, 2, 3, 4, 5], CreatedDays(actual));
        }

        [Fact]
        public void ShouldIncludeEntitiesCreatedAtCreatedFrom()
        {
            var actual = Entities().ApplyDateFilter(new DateFilter(CreatedFrom: Day(4)));

            Assert.Equal([4, 5], CreatedDays(actual));
        }

        [Fact]
        public void ShouldExcludeEntitiesCreatedAtCreatedTo()
        {
            var actual = Entities().ApplyDateFilter(new DateFilter(CreatedTo: Day(3)));

            Assert.Equal([1, 2], CreatedDays(actual));
        }

        [Fact]
        public void ShouldIncludeEntitiesModifiedAtModifiedFrom()
        {
            var actual = Entities().ApplyDateFilter(new DateFilter(ModifiedFrom: Day(14)));

            Assert.Equal([4, 5], CreatedDays(actual));
        }

        [Fact]
        public void ShouldExcludeEntitiesModifiedAtModifiedTo()
        {
            var actual = Entities().ApplyDateFilter(new DateFilter(ModifiedTo: Day(13)));

            Assert.Equal([1, 2], CreatedDays(actual));
        }

        [Fact]
        public void ShouldFilterByCreatedRange_WhenCreatedFromAndCreatedToAreProvided()
        {
            var actual = Entities().ApplyDateFilter(new DateFilter(CreatedFrom: Day(2), CreatedTo: Day(5)));

            Assert.Equal([2, 3, 4], CreatedDays(actual));
        }

        [Fact]
        public void ShouldFilterByModifiedRange_WhenModifiedFromAndModifiedToAreProvided()
        {
            var actual = Entities()
                .ApplyDateFilter(new DateFilter(ModifiedFrom: Day(13), ModifiedTo: Day(15)));

            Assert.Equal([3, 4], CreatedDays(actual));
        }

        [Fact]
        public void ShouldOnlyKeepEntitiesThatMatchEveryValue_WhenCreatedAndModifiedAreProvided()
        {
            var filter = new DateFilter(CreatedFrom: Day(2), CreatedTo: Day(5), ModifiedFrom: Day(13));

            var actual = Entities().ApplyDateFilter(filter);

            Assert.Equal([3, 4], CreatedDays(actual));
        }
    }

    public class ApplyCreatedFromFilter
    {
        [Fact]
        public void ShouldNotFilter_WhenValueIsNull()
        {
            var actual = CreatedEntities().ApplyCreatedFromFilter(null);

            Assert.Equal([Jan1, Jan2, Jan3], actual.Select(e => e.DateCreated));
        }

        [Fact]
        public void ShouldIncludeEntitiesCreatedAtTheValue()
        {
            var actual = CreatedEntities().ApplyCreatedFromFilter(Jan2);

            Assert.Equal([Jan2, Jan3], actual.Select(e => e.DateCreated));
        }

        [Fact]
        public void ShouldExcludeEntitiesCreatedBeforeTheValue()
        {
            var actual = CreatedEntities().ApplyCreatedFromFilter(Jan3.AddTicks(1));

            Assert.Empty(actual);
        }
    }

    public class ApplyCreatedToFilter
    {
        [Fact]
        public void ShouldNotFilter_WhenValueIsNull()
        {
            var actual = CreatedEntities().ApplyCreatedToFilter(null);

            Assert.Equal([Jan1, Jan2, Jan3], actual.Select(e => e.DateCreated));
        }

        [Fact]
        public void ShouldExcludeEntitiesCreatedAtTheValue()
        {
            var actual = CreatedEntities().ApplyCreatedToFilter(Jan3);

            Assert.Equal([Jan1, Jan2], actual.Select(e => e.DateCreated));
        }

        [Fact]
        public void ShouldIncludeEntitiesCreatedJustBeforeTheValue()
        {
            var actual = CreatedEntities().ApplyCreatedToFilter(Jan1.AddTicks(1));

            Assert.Equal([Jan1], actual.Select(e => e.DateCreated));
        }
    }

    public class ApplyModifiedFromFilter
    {
        [Fact]
        public void ShouldNotFilter_WhenValueIsNull()
        {
            var actual = ModifiedEntities().ApplyModifiedFromFilter(null);

            Assert.Equal([Jan1, Jan2, Jan3], actual.Select(e => e.DateModified));
        }

        [Fact]
        public void ShouldIncludeEntitiesModifiedAtTheValue()
        {
            var actual = ModifiedEntities().ApplyModifiedFromFilter(Jan2);

            Assert.Equal([Jan2, Jan3], actual.Select(e => e.DateModified));
        }

        [Fact]
        public void ShouldExcludeEntitiesModifiedBeforeTheValue()
        {
            var actual = ModifiedEntities().ApplyModifiedFromFilter(Jan3.AddTicks(1));

            Assert.Empty(actual);
        }
    }

    public class ApplyModifiedToFilter
    {
        [Fact]
        public void ShouldNotFilter_WhenValueIsNull()
        {
            var actual = ModifiedEntities().ApplyModifiedToFilter(null);

            Assert.Equal([Jan1, Jan2, Jan3], actual.Select(e => e.DateModified));
        }

        [Fact]
        public void ShouldExcludeEntitiesModifiedAtTheValue()
        {
            var actual = ModifiedEntities().ApplyModifiedToFilter(Jan3);

            Assert.Equal([Jan1, Jan2], actual.Select(e => e.DateModified));
        }

        [Fact]
        public void ShouldIncludeEntitiesModifiedJustBeforeTheValue()
        {
            var actual = ModifiedEntities().ApplyModifiedToFilter(Jan1.AddTicks(1));

            Assert.Equal([Jan1], actual.Select(e => e.DateModified));
        }
    }
}