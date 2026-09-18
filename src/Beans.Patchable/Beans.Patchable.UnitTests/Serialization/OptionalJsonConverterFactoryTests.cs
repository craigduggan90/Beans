using Beans.Patchable.Serialization;
using Beans.Patchable.UnitTests.Serialization.Dtos;
using System.Text.Json;

namespace Beans.Patchable.UnitTests.Serialization;

public static class OptionalJsonConverterFactoryTests
{
    private static JsonSerializerOptions OptionsWithFactory() => new()
    {
        Converters = { new OptionalJsonConverterFactory() }
    };

    public class CanConvert
    {
        [Theory]
        [InlineData(typeof(Optional<string>))]
        [InlineData(typeof(Optional<int>))]
        [InlineData(typeof(Optional<int?>))]
        [InlineData(typeof(Optional<Nested>))]
        public void WhenTypeIsOptionalOfAnything_ReturnsTrue(Type optionalType)
        {
            var factory = new OptionalJsonConverterFactory();

            Assert.True(factory.CanConvert(optionalType));
        }

        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(Nested))]
        [InlineData(typeof(List<string>))]
        [InlineData(typeof(Nullable<int>))]
        public void WhenTypeIsNotOptional_ReturnsFalse(Type nonOptionalType)
        {
            var factory = new OptionalJsonConverterFactory();

            Assert.False(factory.CanConvert(nonOptionalType));
        }
    }

    public class CreateConverter
    {
        [Fact]
        public void ForOptionalOfString_ReturnsConverterForThatType()
        {
            var factory = new OptionalJsonConverterFactory();

            var converter = factory.CreateConverter(typeof(Optional<string?>), new JsonSerializerOptions());

            Assert.IsType<OptionalJsonConverter<string?>>(converter);
        }

        [Fact]
        public void ForOptionalOfInt_ReturnsConverterForThatType()
        {
            var factory = new OptionalJsonConverterFactory();

            var converter = factory.CreateConverter(typeof(Optional<int>), new JsonSerializerOptions());

            Assert.IsType<OptionalJsonConverter<int>>(converter);
        }

        [Fact]
        public void ForOptionalOfComplexType_ReturnsConverterForThatType()
        {
            var factory = new OptionalJsonConverterFactory();

            var converter = factory.CreateConverter(typeof(Optional<Nested>), new JsonSerializerOptions());

            Assert.IsType<OptionalJsonConverter<Nested>>(converter);
        }
    }

    public class Integration
    {
        // These confirm the factory does its actual job: registering it once, without registering
        // OptionalJsonConverter<T> per-type, is enough for JsonSerializer to handle any Optional<T>.

        [Fact]
        public void WhenFactoryRegistered_DeserializesOptionalOfStringWithoutExplicitConverter()
        {
            var options = OptionsWithFactory();

            var dto = JsonSerializer.Deserialize<StringDto>("""{"Property":"hello"}""", options);

            Assert.True(dto!.Property.IsSet);
            Assert.Equal("hello", dto.Property.Value);
        }

        [Fact]
        public void WhenFactoryRegistered_DeserializesOptionalOfIntWithoutExplicitConverter()
        {
            var options = OptionsWithFactory();

            var dto = JsonSerializer.Deserialize<IntDto>("""{"Property":42}""", options);

            Assert.True(dto!.Property.IsSet);
            Assert.Equal(42, dto.Property.Value);
        }

        [Fact]
        public void WhenFactoryRegistered_PropertyOmitted_ReturnsUnsetOptional()
        {
            var options = OptionsWithFactory();

            var dto = JsonSerializer.Deserialize<IntDto>("""{}""", options);

            Assert.False(dto!.Property.IsSet);
        }

        [Fact]
        public void WhenFactoryRegistered_DeserializesOptionalOfComplexTypeWithoutExplicitConverter()
        {
            var options = OptionsWithFactory();

            var dto = JsonSerializer.Deserialize<ComplexDto>(
                """{"Property":{"Name":"Alice","Value":7}}""", options);

            Assert.True(dto!.Property.IsSet);
            Assert.Equal("Alice", dto.Property.Value!.Name);
            Assert.Equal(7, dto.Property.Value!.Value);
        }

        [Fact]
        public void WhenFactoryRegistered_SerializesSetValue()
        {
            var options = OptionsWithFactory();
            var dto = new IntDto(Optional<int>.Of(99));

            var json = JsonSerializer.Serialize(dto, options);

            Assert.Equal("""{"Property":99}""", json);
        }
    }
}