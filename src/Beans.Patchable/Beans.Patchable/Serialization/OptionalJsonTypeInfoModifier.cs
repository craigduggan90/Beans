using System.Text.Json.Serialization.Metadata;

namespace Beans.Patchable.Serialization;

/// <summary>A <see cref="JsonTypeInfo"/> contract modifier that omits unset <see cref="Optional{T}"/> properties from serialized JSON.</summary>
public static class OptionalJsonTypeInfoModifier
{
    /// <summary>Marks every <see cref="Optional{T}"/> property on <paramref name="typeInfo"/> to be serialized only when it is set.</summary>
    /// <param name="typeInfo">The type contract to modify.</param>
    public static void OmitUnsetOptionals(JsonTypeInfo typeInfo)
    {
        foreach (var property in typeInfo.Properties)
        {
            if (!IsOptional(property.PropertyType))
                continue;

            var isSetProperty = property.PropertyType.GetProperty(nameof(Optional<object>.IsSet))!;
            property.ShouldSerialize = (_, value) => (bool)isSetProperty.GetValue(value)!;
        }
    }

    /// <summary>Determines whether a property's declared type is <see cref="Optional{T}"/> for some <c>T</c>.</summary>
    /// <param name="type">The declared property type to inspect.</param>
    /// <returns><see langword="true"/> when the type is a closed <see cref="Optional{T}"/>; otherwise, <see langword="false"/>.</returns>
    private static bool IsOptional(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Optional<>);
}
