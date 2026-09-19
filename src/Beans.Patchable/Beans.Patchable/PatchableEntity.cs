using System.Reflection;

namespace Beans.Patchable;

/// <summary>
/// Base class for entities supporting Patch-style updates via <see cref="Optional{T}"/> values.
/// </summary>
public abstract class PatchableEntity : IPatchableEntity
{
    /// <summary>Provides access to the nullable-reference-type metadata emitted by the compiler.</summary>
    private static readonly NullabilityInfoContext NullabilityContext = new();

    /// <inheritdoc/>
    public bool IsDirty { get; private set; }

    /// <inheritdoc />
    public virtual bool UpdateProperty<T>(string propertyName, Optional<T> optionalValue)
    {
        // An unset Optional represents "this property was not part of the patch", which is deliberately
        // different from explicitly setting a property to null.
        if (!optionalValue.TryGetValue(out var value))
            return false;

        var property = GetProperty(propertyName);

        // The runtime type check is performed before handling null because a null value carries no runtime
        // type information of its own.
        if (!IsCompatible(optionalValue.ValueType, property.PropertyType))
        {
            throw PatchableException.ForIncorrectPropertyType(
                GetType(),
                propertyName,
                property.PropertyType,
                optionalValue.ValueType);
        }

        // Null requires separate handling because assigning null has different semantics from assigning an
        // ordinary value: the target property must explicitly permit null.
        return value is null
            ? SetNull(property)
            : SetValue(property, value);
    }

    /// <inheritdoc />
    public virtual void SetDirty() => IsDirty = true;

    /// <summary>Determines whether a property permits a null value. </summary>
    /// <param name="propertyInfo">The property to inspect.</param>
    /// <returns><see langword="true"/> when the property is nullable; otherwise, <see langword="false"/>.</returns>
    private static bool IsNullable(PropertyInfo propertyInfo)
    {
        // Nullable value types such as int? are represented by Nullable<T> at runtime, so their nullability
        // can be determined directly from the type.
        if (Nullable.GetUnderlyingType(propertyInfo.PropertyType) is not null)
            return true;

        // Nullable reference types such as string? are represented at runtime by the same Type as string.
        // The compiler's nullable metadata must therefore be inspected to distinguish string from string?.
        var nullability = NullabilityContext.Create(propertyInfo);

        return nullability.WriteState == NullabilityState.Nullable;
    }

    /// <summary>Determines whether a supplied value type can be assigned to a property.</summary>
    /// <param name="providedType">The type of the supplied value.</param>
    /// <param name="propertyType">The declared type of the target property.</param>
    /// <returns>
    /// <see langword="true"/> when the supplied type is compatible with the target property; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    private static bool IsCompatible(Type providedType, Type propertyType)
    {
        // This covers normal reference-type inheritance, interface implementations, and direct value-type matches.
        if (providedType.IsAssignableTo(propertyType))
            return true;

        // Nullable<T> is a wrapper around T at runtime. For example, an int value is valid for an int? property
        // even though typeof(int) is not directly assignable to typeof(int?).
        var underlyingType = Nullable.GetUnderlyingType(propertyType);

        return underlyingType is not null && providedType.IsAssignableTo(underlyingType);
    }

    /// <summary>Gets the readable and writable public instance property with the specified name.</summary>
    /// <param name="propertyName">The name of the property to find.</param>
    /// <returns>The matching property.</returns>
    /// <exception cref="PatchableException">when no property is found with the given name.</exception>
    private PropertyInfo GetProperty(string propertyName)
        => GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p =>
                p.Name.Equals(propertyName, StringComparison.Ordinal) &&
                p is { CanRead: true, CanWrite: true })
            ?? throw PatchableException.ForPropertyNotFoundInType(GetType(), propertyName);

    /// <summary>Sets a property to null.</summary>
    /// <param name="property">The property to update.</param>
    /// <returns><see langword="true"/> when the property was changed; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="PatchableException">when the property cannot be null.</exception>
    private bool SetNull(PropertyInfo property)
    {
        if (!IsNullable(property))
            throw PatchableException.ForNullAssignedToNonNullableProperty(GetType(), property.Name);

        // Setting a property to the value it already contains is not a change, so it must not mark the entity dirty.
        if (property.GetValue(this) is null)
            return false;

        property.SetValue(this, null);
        IsDirty = true;

        return true;
    }

    /// <summary>Assigns a given value to a property.</summary>
    /// <param name="property">The property to update.</param>
    /// <param name="value">The value to assign.</param>
    /// <returns><see langword="true"/> when the property was changed; otherwise, <see langword="false"/>.</returns>
    private bool SetValue(PropertyInfo property, object value)
    {
        var currentValue = property.GetValue(this);

        // Equality is checked before reflection performs the assignment so that an idempotent patch does not
        // unnecessarily mark the entity dirty.
        if (Equals(value, currentValue))
            return false;

        property.SetValue(this, value);
        IsDirty = true;

        return true;
    }
}