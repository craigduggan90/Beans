using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Beans.Pageable.IntegrationTests.TestSupport;

/// <summary>Stores <see cref="DateTime"/> values as UTC, and reads them back with <see cref="DateTimeKind.Utc"/>.</summary>
public class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    convertToProviderExpression: value => value.ToUniversalTime(),
    convertFromProviderExpression: value => DateTime.SpecifyKind(value, DateTimeKind.Utc));