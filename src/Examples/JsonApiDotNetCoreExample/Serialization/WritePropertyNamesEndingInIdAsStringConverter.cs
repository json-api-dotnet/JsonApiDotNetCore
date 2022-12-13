using System.Text.Json;
using System.Text.Json.Serialization;
using JsonApiDotNetCore.Serialization.JsonConverters;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCoreExample.Serialization;

public sealed class WritePropertyNamesEndingInIdAsStringConverter : JsonConverter<ResourceObject>
{
    private static readonly JsonEncodedText TypeText = JsonEncodedText.Encode("type");
    private static readonly JsonEncodedText IdText = JsonEncodedText.Encode("id");
    private static readonly JsonEncodedText LidText = JsonEncodedText.Encode("lid");
    private static readonly JsonEncodedText MetaText = JsonEncodedText.Encode("meta");
    private static readonly JsonEncodedText AttributesText = JsonEncodedText.Encode("attributes");
    private static readonly JsonEncodedText RelationshipsText = JsonEncodedText.Encode("relationships");
    private static readonly JsonEncodedText LinksText = JsonEncodedText.Encode("links");

    private readonly ResourceObjectConverter _innerConverter;

    public WritePropertyNamesEndingInIdAsStringConverter(ResourceObjectConverter innerConverter)
    {
        ArgumentNullException.ThrowIfNull(innerConverter);

        _innerConverter = innerConverter;
    }

    public override ResourceObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return _innerConverter.Read(ref reader, typeToConvert, options);
    }

    public override void Write(Utf8JsonWriter writer, ResourceObject value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString(TypeText, value.Type);

        if (value.Id != null)
        {
            writer.WriteString(IdText, value.Id);
        }

        if (value.Lid != null)
        {
            writer.WriteString(LidText, value.Lid);
        }

        if (value.Attributes != null && value.Attributes.Any())
        {
            WriteAttributes(writer, value.Attributes, options);
        }

        if (value.Relationships != null && value.Relationships.Any())
        {
            writer.WritePropertyName(RelationshipsText);
            WriteSubTree(writer, value.Relationships, options);
        }

        if (value.Links != null && !string.IsNullOrEmpty(value.Links.Self))
        {
            writer.WritePropertyName(LinksText);
            WriteSubTree(writer, value.Links, options);
        }

        if (value.Meta != null && value.Meta.Any())
        {
            writer.WritePropertyName(MetaText);
            WriteSubTree(writer, value.Meta, options);
        }

        writer.WriteEndObject();
    }

    private static void WriteAttributes(Utf8JsonWriter writer, IDictionary<string, object?> attributes, JsonSerializerOptions options)
    {
        writer.WriteStartObject(AttributesText);

        foreach ((string attributeName, object? attributeValue) in attributes)
        {
            if (attributeValue == null)
            {
                writer.WriteNull(attributeName);
            }
            else
            {
                writer.WritePropertyName(attributeName);

                object? outputAttributeValue = ShouldWriteAttributeValueAsString(attributeName) ? attributeValue.ToString() : attributeValue;
                WriteSubTree(writer, outputAttributeValue, options);
            }
        }

        writer.WriteEndObject();
    }

    private static bool ShouldWriteAttributeValueAsString(string attributeName)
    {
        return attributeName.Length > 2 && attributeName.EndsWith("Id", StringComparison.Ordinal);
    }

    private static void WriteSubTree<TValue>(Utf8JsonWriter writer, TValue value, JsonSerializerOptions options)
    {
        if (typeof(TValue) != typeof(object) && options.GetConverter(typeof(TValue)) is JsonConverter<TValue> converter)
        {
            converter.Write(writer, value, options);
        }
        else
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
