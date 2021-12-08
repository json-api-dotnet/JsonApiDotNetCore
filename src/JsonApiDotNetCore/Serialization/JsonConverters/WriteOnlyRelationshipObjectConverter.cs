using System.Text.Json;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.JsonConverters;

/// <summary>
/// Converts <see cref="RelationshipObject" /> to JSON.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class WriteOnlyRelationshipObjectConverter : JsonObjectConverter<RelationshipObject>
{
    private static readonly JsonEncodedText DataText = JsonEncodedText.Encode("data");
    private static readonly JsonEncodedText LinksText = JsonEncodedText.Encode("links");
    private static readonly JsonEncodedText MetaText = JsonEncodedText.Encode("meta");

    public override RelationshipObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException("This converter cannot be used for reading JSON.");
    }

    /// <summary>
    /// Conditionally writes <code>"data": null</code> or omits it, depending on <see cref="SingleOrManyData{TObject}.IsAssigned" />.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, RelationshipObject value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        if (value.Links != null && value.Links.HasValue())
        {
            writer.WritePropertyName(LinksText);
            WriteSubTree(writer, value.Links, options);
        }

        if (value.Data.IsAssigned)
        {
            writer.WritePropertyName(DataText);
            WriteSubTree(writer, value.Data, options);
        }

        if (!value.Meta.IsNullOrEmpty())
        {
            writer.WritePropertyName(MetaText);
            WriteSubTree(writer, value.Meta, options);
        }

        writer.WriteEndObject();
    }
}
