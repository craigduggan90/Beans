using Beans.Patchable.Serialization;
using Beans.Patchable.UnitTests.Serialization.Dtos;
using System.Text.Json;

namespace Beans.Patchable.UnitTests.Serialization;

public static class OptionalJsonConverterTests
{
    private static JsonSerializerOptions OptionsFor<T>() => new()
    {
        Converters = { new OptionalJsonConverter<T>() }
    };

    public class Read
    {
        [Fact]
        public void WhenPropertyPresentWithValue_ReturnsSetOptionalWithValue()
        {
            var options = OptionsFor<string?>();

            var dto = JsonSerializer.Deserialize<StringDto>("""{"Property":"hello"}""", options);

            Assert.True(dto!.Property.IsSet);
            Assert.Equal("hello", dto.Property.Value);
        }

        [Fact]
        public void WhenPropertyPresentAsExplicitNull_ReturnsSetOptionalWithNullValue()
        {
            var options = OptionsFor<string?>();

            var dto = JsonSerializer.Deserialize<StringDto>("""{"Property":null}""", options);

            Assert.True(dto!.Property.IsSet);
            Assert.Null(dto.Property.Value);
        }

        [Fact]
        public void WhenPropertyOmitted_ReturnsUnsetOptional()
        {
            var options = OptionsFor<string?>();

            var dto = JsonSerializer.Deserialize<StringDto>("""{}""", options);

            Assert.False(dto!.Property.IsSet);
        }

        [Fact]
        public void WhenPropertyOmitted_ValueIsNull()
        {
            var options = OptionsFor<string?>();

            var dto = JsonSerializer.Deserialize<StringDto>("""{}""", options);

            Assert.Null(dto!.Property.Value);
        }

        [Fact]
        public void WithNonNullableValueTypeAndExplicitNull_ThrowsJsonException()
        {
            // int is not nullable, so a JSON null can't be deserialized into it — this should
            // surface as a JsonException rather than silently producing Optional<int>.Of(0).
            var options = OptionsFor<int>();

            Assert.Throws<JsonException>(() =>
                JsonSerializer.Deserialize<IntDto>("""{"Property":null}""", options));
        }

        [Fact]
        public void WithNullableValueTypeAndExplicitNull_ReturnsSetOptionalWithNullValue()
        {
            var options = OptionsFor<int?>();

            var dto = JsonSerializer.Deserialize<NullableIntDto>("""{"Property":null}""", options);

            Assert.True(dto!.Property.IsSet);
            Assert.Null(dto.Property.Value);
        }

        [Fact]
        public void WithValueType_ReturnsSetOptionalWithValue()
        {
            var options = OptionsFor<int>();

            var dto = JsonSerializer.Deserialize<IntDto>("""{"Property":42}""", options);

            Assert.True(dto!.Property.IsSet);
            Assert.Equal(42, dto.Property.Value);
        }

        [Fact]
        public void WithComplexType_DeserializesNestedObject()
        {
            var options = OptionsFor<Nested>();

            var dto = JsonSerializer.Deserialize<ComplexDto>(
                """{"Property":{"Name":"Alice","Value":7}}""", options);

            Assert.True(dto!.Property.IsSet);
            Assert.Equal("Alice", dto.Property.Value!.Name);
            Assert.Equal(7, dto.Property.Value!.Value);
        }
    }

    public class Write
    {
        [Fact]
        public void WhenSetWithValue_SerializesValue()
        {
            var options = OptionsFor<string?>();
            var dto = new StringDto(Optional<string?>.Of("hello"));

            var json = JsonSerializer.Serialize(dto, options);

            Assert.Equal("""{"Property":"hello"}""", json);
        }

        [Fact]
        public void WhenSetWithNull_SerializesNull()
        {
            var options = OptionsFor<string?>();
            var dto = new StringDto(Optional<string?>.Of(null));

            var json = JsonSerializer.Serialize(dto, options);

            Assert.Equal("""{"Property":null}""", json);
        }

        [Fact]
        public void WhenUnset_SerializesNull()
        {
            // Write() reads value.Value unconditionally, and Value is default/null when IsSet is
            // false, so an unset Optional<T> serializes the same way as one explicitly set to null.
            var options = OptionsFor<string?>();
            var dto = new StringDto(Optional<string?>.Unset);

            var json = JsonSerializer.Serialize(dto, options);

            Assert.Equal("""{"Property":null}""", json);
        }
    }

    public class RoundTrip
    {
        [Fact]
        public void SetValue_PreservesValueAndIsSet()
        {
            var options = OptionsFor<int>();
            var original = new IntDto(Optional<int>.Of(99));

            var json = JsonSerializer.Serialize(original, options);
            var roundTripped = JsonSerializer.Deserialize<IntDto>(json, options);

            Assert.True(roundTripped!.Property.IsSet);
            Assert.Equal(99, roundTripped.Property.Value);
        }

        [Fact]
        public void ExplicitNull_PreservesIsSetTrueAndNullValue()
        {
            var options = OptionsFor<string?>();
            var original = new StringDto(Optional<string?>.Of(null));

            var json = JsonSerializer.Serialize(original, options);
            var roundTripped = JsonSerializer.Deserialize<StringDto>(json, options);

            Assert.True(roundTripped!.Property.IsSet);
            Assert.Null(roundTripped.Property.Value);
        }
    }
}