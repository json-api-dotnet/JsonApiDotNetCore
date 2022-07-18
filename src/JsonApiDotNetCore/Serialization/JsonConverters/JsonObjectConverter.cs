using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonApiDotNetCore.Serialization.JsonConverters;

public abstract class JsonObjectConverter<TObject> : JsonConverter<TObject>
{
    protected static TValue? ReadSubTree<TValue>(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<TValue>(ref reader, options);
    }

    protected static void WriteSubTree<TValue>(Utf8JsonWriter writer, TValue value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }

    protected static JsonException GetEndOfStreamError()
    {
        return new JsonException("Unexpected end of JSON stream.");
    }
}
