using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCore.Serialization
{
    internal static class JsonConverterSupport
    {
        public static T ReadSubTree<T>(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (typeof(T) != typeof(object) && options?.GetConverter(typeof(T)) is JsonConverter<T> converter)
            {
                return converter.Read(ref reader, typeof(T), options);
            }

            return JsonSerializer.Deserialize<T>(ref reader, options);
        }

        public static void WriteSubTree<T>(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (typeof(T) != typeof(object) && options?.GetConverter(typeof(T)) is JsonConverter<T> converter)
            {
                converter.Write(writer, value, options);
            }
            else
            {
                JsonSerializer.Serialize(writer, value, options);
            }
        }

        public static JsonException GetEndOfStreamError()
        {
            return new("Unexpected end of JSON stream.");
        }
    }
}
