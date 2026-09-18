using System.Text.Json;
using System.Text.Json.Serialization;

namespace Beans.Patchable.Serialization;

/// <summary>Converts <see cref="Optional{T}"/> to and from JSON via its underlying value.</summary>
public class OptionalJsonConverter<T> : JsonConverter<Optional<T>>
{
    /// <summary>Reads a JSON value into a set <see cref="Optional{T}"/>.</summary>
    public override Optional<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => Optional<T>.Of(JsonSerializer.Deserialize<T>(ref reader, options));

    /// <summary>Writes the underlying value, or <see langword="null"/> when unset.</summary>
    public override void Write(Utf8JsonWriter writer, Optional<T> value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value.Value, options);
}