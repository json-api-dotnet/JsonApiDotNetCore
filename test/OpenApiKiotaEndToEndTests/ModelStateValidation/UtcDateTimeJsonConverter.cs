using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenApiKiotaEndToEndTests.ModelStateValidation;

internal class UtcDateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.Parse(reader.GetString()!);
        return dateTimeOffset.UtcDateTime;
    }

    public override void Write(Utf8JsonWriter writer, DateTime val, JsonSerializerOptions options)
    {
        writer.WriteStringValue(val.ToUniversalTime().ToString("O"));
    }
}
