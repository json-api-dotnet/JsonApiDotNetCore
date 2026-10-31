using System.Text.Json;
using System.Text.Json.Serialization;
using JsonApiDotNetCore.Serialization.Response;

namespace JsonApiDotNetCore.Serialization.JsonConverters;

/// <summary>
/// Writes a <see cref="CompoundObjectValue" /> to JSON, using the member names verbatim (bypassing
/// <see cref="JsonSerializerOptions.DictionaryKeyPolicy" />).
/// </summary>
internal sealed class CompoundObjectValueConverter : JsonConverter<CompoundObjectValue>
{
    public override CompoundObjectValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException("Reading a compound object value is not supported.");
    }

    public override void Write(Utf8JsonWriter writer, CompoundObjectValue value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach ((string memberName, object? memberValue) in value)
        {
            writer.WritePropertyName(memberName);

            if (memberValue == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                JsonSerializer.Serialize(writer, memberValue, memberValue.GetType(), options);
            }
        }

        writer.WriteEndObject();
    }
}
