using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenApiTests;

internal abstract class ReferenceTypeJsonConverter<T>(Func<string, T> fromStringConverter, Func<T, string> toStringConverter) : JsonConverter<T>
    where T : class
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            try
            {
                string stringValue = reader.GetString()!;
                return fromStringConverter(stringValue);
            }
            catch (Exception exception) when (exception is FormatException or OverflowException or InvalidCastException or ArgumentException)
            {
                throw new JsonException("Failed to parse attribute value.", exception);
            }
        }

        throw new JsonException($"Expected JSON token type '{JsonTokenType.String}' or '{JsonTokenType.Null}', but found '{reader.TokenType}'.");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        try
        {
            string stringValue = toStringConverter(value);
            writer.WriteStringValue(stringValue);
        }
        catch (Exception exception) when (exception is FormatException or OverflowException or InvalidCastException or ArgumentException)
        {
            throw new JsonException("Failed to format attribute value.", exception);
        }
    }
}
