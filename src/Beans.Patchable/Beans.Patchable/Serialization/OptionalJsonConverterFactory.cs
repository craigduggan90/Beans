using System.Text.Json;
using System.Text.Json.Serialization;

namespace Beans.Patchable.Serialization;

/// <summary>Creates an <see cref="OptionalJsonConverter{T}"/> for any closed <see cref="Optional{T}"/> type.</summary>
public class OptionalJsonConverterFactory : JsonConverterFactory
{
    /// <summary>Determines whether <paramref name="typeToConvert"/> is a closed <see cref="Optional{T}"/>.</summary>
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsGenericType &&
           typeToConvert.GetGenericTypeDefinition() == typeof(Optional<>);

    /// <summary>Creates an <see cref="OptionalJsonConverter{T}"/> matching <paramref name="typeToConvert"/>'s type argument.</summary>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var innerType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(OptionalJsonConverter<>).MakeGenericType(innerType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}