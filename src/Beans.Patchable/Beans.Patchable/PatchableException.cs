namespace Beans.Patchable;

/// <summary>
/// Represents an error encountered while applying a property update to a <see cref="PatchableEntity"/>.
/// </summary>
/// <param name="message">A message describing the error.</param>
/// <param name="innerException">The exception that caused this exception, if applicable.</param>
public class PatchableException(string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    /// <summary>
    /// Creates an exception indicating that a supplied value has an incompatible type for the target property.
    /// </summary>
    /// <param name="type">The type of the entity containing the property.</param>
    /// <param name="propertyName">The name of the property being updated.</param>
    /// <param name="expectedType">The type expected by the property.</param>
    /// <param name="providedType">The type supplied by the update.</param>
    /// <returns>A <see cref="PatchableException"/> describing the type mismatch.</returns>
    internal static PatchableException ForIncorrectPropertyType(
        Type type,
        string propertyName,
        Type expectedType,
        Type providedType) =>
        new(GetIncorrectPropertyTypeMessage(propertyName, type.Name, expectedType.Name, providedType.Name));

    /// <summary>
    /// Creates an exception indicating that the requested property does not exist or cannot be updated.
    /// </summary>
    /// <param name="type">The type of the entity on which the property was requested.</param>
    /// <param name="propertyName">The name of the property that could not be found.</param>
    /// <returns>A <see cref="PatchableException"/> describing the missing property.</returns>
    internal static PatchableException ForPropertyNotFoundInType(Type type, string propertyName) =>
        new(GetPropertyNotFoundInTypeMessage(propertyName, type.Name));

    /// <summary>
    /// Creates an exception indicating that <see langword="null"/> was assigned to a non-nullable property.
    /// </summary>
    /// <param name="type">The type of the entity containing the property.</param>
    /// <param name="propertyName">The name of the property being updated.</param>
    /// <returns>A <see cref="PatchableException"/> describing the invalid null assignment.</returns>
    internal static PatchableException ForNullAssignedToNonNullableProperty(Type type, string propertyName) =>
        new(GetNullAssignedToNonNullablePropertyMessage(propertyName, type.Name));

    private static string GetIncorrectPropertyTypeMessage(
        string propertyName,
        string subjectType,
        string expectedType,
        string providedType) =>
        $"Failed to set property '{propertyName}' on type '{subjectType}': " +
        $"Incorrect value type (expected {expectedType}, received {providedType}).";

    private static string GetPropertyNotFoundInTypeMessage(string propertyName, string subjectType) =>
        $"Failed to set property '{propertyName}' on type '{subjectType}': Property not found.";

    private static string GetNullAssignedToNonNullablePropertyMessage(
        string propertyName,
        string subjectType) =>
        $"Failed to set property '{propertyName}' on type '{subjectType}': " +
        "Cannot assign null to a non-nullable property.";
}