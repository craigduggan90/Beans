namespace Beans.Pageable;

/// <summary>Model containing filter values for entity created and last modified timestamps.</summary>
/// <param name="CreatedFrom">Limit results to entities created on or after this date and time (inclusive).</param>
/// <param name="CreatedTo">Limit results to entities created before this date and time (exclusive).</param>
/// <param name="ModifiedFrom">Limit results to entities last modified on or after this date and time (inclusive).</param>
/// <param name="ModifiedTo">Limit results to entities last modified before this date and time (exclusive).</param>
public record DateFilter(
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null,
    DateTime? ModifiedFrom = null,
    DateTime? ModifiedTo = null);