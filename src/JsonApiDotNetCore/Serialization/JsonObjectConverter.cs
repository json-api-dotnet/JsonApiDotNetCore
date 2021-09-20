using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonApiDotNetCore.Serialization
{
    public abstract class JsonObjectConverter<TObject> : JsonConverter<TObject>
    {
        protected static TValue ReadSubTree<TValue>(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (typeof(TValue) != typeof(object) && options?.GetConverter(typeof(TValue)) is JsonConverter<TValue> converter)
            {
                return converter.Read(ref reader, typeof(TValue), options);
            }

            return JsonSerializer.Deserialize<TValue>(ref reader, options);
        }

        protected static void WriteSubTree<TValue>(Utf8JsonWriter writer, TValue value, JsonSerializerOptions options)
        {
            if (typeof(TValue) != typeof(object) && options?.GetConverter(typeof(TValue)) is JsonConverter<TValue> converter)
            {
                converter.Write(writer, value, options);
            }
            else
            {
                JsonSerializer.Serialize(writer, value, options);
            }
        }

        protected static JsonException GetEndOfStreamError()
        {
            return new("Unexpected end of JSON stream.");
        }
    }
}
