using Beans.Patchable.Serialization;
using Beans.Patchable.UnitTests.Serialization.Dtos;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Beans.Patchable.UnitTests.Serialization;

public static class OptionalJsonTypeInfoModifierTests
{
    private static JsonSerializerOptions OptionsWithModifier() => new()
    {
        Converters = { new OptionalJsonConverterFactory() },
        TypeInfoResolver = new DefaultJsonTypeInfoResolver().WithAddedModifier(OptionalJsonTypeInfoModifier.OmitUnsetOptionals)
    };

    public class OmitUnsetOptionals
    {
        [Fact]
        public void WhenUnset_OmitsPropertyFromOutput()
        {
            var options = OptionsWithModifier();
            var dto = new StringDto(Optional<string?>.Unset);

            var json = JsonSerializer.Serialize(dto, options);

            Assert.Equal("{}", json);
        }

        [Fact]
        public void WhenSetToNull_IncludesPropertyAsNull()
        {
            var options = OptionsWithModifier();
            var dto = new StringDto(Optional<string?>.Of(null));

            var json = JsonSerializer.Serialize(dto, options);

            Assert.Equal("""{"Property":null}""", json);
        }

        [Fact]
        public void WhenSetToValue_IncludesPropertyWithValue()
        {
            var options = OptionsWithModifier();
            var dto = new StringDto(Optional<string?>.Of("hello"));

            var json = JsonSerializer.Serialize(dto, options);

            Assert.Equal("""{"Property":"hello"}""", json);
        }

        [Fact]
        public void WhenSetToDefaultValueOfInnerType_StillIncludesProperty()
        {
            // Confirms the modifier checks IsSet rather than comparing the whole Optional<T> to its own
            // default, so a value type explicitly set to its default (0) is not mistaken for Unset.
            var options = OptionsWithModifier();
            var dto = new IntDto(Optional<int>.Of(0));

            var json = JsonSerializer.Serialize(dto, options);

            Assert.Equal("""{"Property":0}""", json);
        }

        [Fact]
        public void WhenModifierNotApplied_UnsetPropertyIsSerializedAsNullInstead()
        {
            // Documents the baseline the modifier changes: without it, an unset Optional<T> still
            // serializes (as null via OptionalJsonConverter<T>) rather than being omitted.
            var options = new JsonSerializerOptions
            {
                Converters = { new OptionalJsonConverterFactory() }
            };
            var dto = new StringDto(Optional<string?>.Unset);

            var json = JsonSerializer.Serialize(dto, options);

            Assert.Equal("""{"Property":null}""", json);
        }

        [Fact]
        public void WhenTypeHasNonOptionalProperties_LeavesThemUnaffected()
        {
            var options = OptionsWithModifier();
            var dto = new MixedDto(Optional<string>.Unset, "always here");

            var json = JsonSerializer.Serialize(dto, options);

            Assert.Equal("""{"Plain":"always here"}""", json);
        }
    }
}
