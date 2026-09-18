using Beans.Patchable.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Beans.Patchable.IntegrationTests;

/// <summary>
/// Exercises the full read/write matrix for <see cref="Optional{T}"/> across <see cref="OptionalJsonConverterFactory"/>
/// and <see cref="OptionalJsonTypeInfoModifier"/> working together, rather than either in isolation.
/// </summary>
public static class OptionalJsonSerializationTests
{
    private sealed record Dto(Optional<string?> Property);

    public class Read
    {
        private static JsonSerializerOptions Options => new()
        {
            Converters = { new OptionalJsonConverterFactory() }
        };

        [Fact]
        public void WhenPropertyOmitted_ReturnsUnsetWithNullValue()
        {
            var dto = JsonSerializer.Deserialize<Dto>("{}", Options);

            Assert.False(dto!.Property.IsSet);
            Assert.Null(dto.Property.Value);
        }

        [Fact]
        public void WhenPropertySetToNull_ReturnsSetWithNullValue()
        {
            var dto = JsonSerializer.Deserialize<Dto>("""{"Property":null}""", Options);

            Assert.True(dto!.Property.IsSet);
            Assert.Null(dto.Property.Value);
        }

        [Fact]
        public void WhenPropertySetToValue_ReturnsSetWithValue()
        {
            var dto = JsonSerializer.Deserialize<Dto>("""{"Property":"value"}""", Options);

            Assert.True(dto!.Property.IsSet);
            Assert.Equal("value", dto.Property.Value);
        }
    }

    public class Write
    {
        private static JsonSerializerOptions WithModifier() => new()
        {
            Converters = { new OptionalJsonConverterFactory() },
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
                .WithAddedModifier(OptionalJsonTypeInfoModifier.OmitUnsetOptionals)
        };

        private static JsonSerializerOptions WithoutModifier() => new()
        {
            Converters = { new OptionalJsonConverterFactory() }
        };

        [Fact]
        public void WhenUnsetAndModifierRegistered_OmitsProperty()
        {
            var json = JsonSerializer.Serialize(new Dto(Optional<string?>.Unset), WithModifier());

            Assert.Equal("{}", json);
        }

        [Fact]
        public void WhenUnsetAndModifierNotRegistered_SerializesNull()
        {
            var json = JsonSerializer.Serialize(new Dto(Optional<string?>.Unset), WithoutModifier());

            Assert.Equal("""{"Property":null}""", json);
        }

        [Fact]
        public void WhenSetToNullAndModifierRegistered_SerializesNull()
        {
            var json = JsonSerializer.Serialize(new Dto(Optional<string?>.Of(null)), WithModifier());

            Assert.Equal("""{"Property":null}""", json);
        }

        [Fact]
        public void WhenSetToNullAndModifierNotRegistered_SerializesNull()
        {
            var json = JsonSerializer.Serialize(new Dto(Optional<string?>.Of(null)), WithoutModifier());

            Assert.Equal("""{"Property":null}""", json);
        }

        [Fact]
        public void WhenSetToValueAndModifierRegistered_SerializesValue()
        {
            var json = JsonSerializer.Serialize(new Dto(Optional<string?>.Of("value")), WithModifier());

            Assert.Equal("""{"Property":"value"}""", json);
        }

        [Fact]
        public void WhenSetToValueAndModifierNotRegistered_SerializesValue()
        {
            var json = JsonSerializer.Serialize(new Dto(Optional<string?>.Of("value")), WithoutModifier());

            Assert.Equal("""{"Property":"value"}""", json);
        }
    }
}
