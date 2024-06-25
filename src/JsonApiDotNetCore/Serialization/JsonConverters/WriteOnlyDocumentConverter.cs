using System.Text.Json;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.JsonConverters;

/// <summary>
/// Converts <see cref="Document" /> to JSON.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class WriteOnlyDocumentConverter : JsonObjectConverter<Document>
{
    private static readonly JsonEncodedText JsonApiText = JsonEncodedText.Encode("jsonapi");
    private static readonly JsonEncodedText LinksText = JsonEncodedText.Encode("links");
    private static readonly JsonEncodedText DataText = JsonEncodedText.Encode("data");
    private static readonly JsonEncodedText AtomicOperationsText = JsonEncodedText.Encode("atomic:operations");
    private static readonly JsonEncodedText AtomicResultsText = JsonEncodedText.Encode("atomic:results");
    private static readonly JsonEncodedText ErrorsText = JsonEncodedText.Encode("errors");
    private static readonly JsonEncodedText IncludedText = JsonEncodedText.Encode("included");
    private static readonly JsonEncodedText MetaText = JsonEncodedText.Encode("meta");

    /// <summary>
    /// Always throws a <see cref="NotSupportedException" />. This converter is write-only.
    /// </summary>
    public override Document Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException("This converter cannot be used for reading JSON.");
    }

    /// <summary>
    /// Conditionally writes <code><![CDATA["data": null]]></code> or omits it, depending on <see cref="SingleOrManyData{TObject}.IsAssigned" />.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, Document value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        if (value.JsonApi != null)
        {
            writer.WritePropertyName(JsonApiText);
            WriteSubTree(writer, value.JsonApi, options);
        }

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

        if (!value.Operations.IsNullOrEmpty())
        {
            writer.WritePropertyName(AtomicOperationsText);
            WriteSubTree(writer, value.Operations, options);
        }

        if (!value.Results.IsNullOrEmpty())
        {
            writer.WritePropertyName(AtomicResultsText);
            writer.WriteStartArray();

            foreach (AtomicResultObject result in value.Results)
            {
                writer.WriteStartObject();

                if (result.Data.IsAssigned)
                {
                    writer.WritePropertyName(DataText);
                    WriteSubTree(writer, result.Data, options);
                }

                if (!result.Meta.IsNullOrEmpty())
                {
                    writer.WritePropertyName(MetaText);
                    WriteSubTree(writer, result.Meta, options);
                }

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        if (!value.Errors.IsNullOrEmpty())
        {
            writer.WritePropertyName(ErrorsText);
            WriteSubTree(writer, value.Errors, options);
        }

        if (value.Included != null)
        {
            writer.WritePropertyName(IncludedText);
            WriteSubTree(writer, value.Included, options);
        }

        if (!value.Meta.IsNullOrEmpty())
        {
            writer.WritePropertyName(MetaText);
            WriteSubTree(writer, value.Meta, options);
        }

        writer.WriteEndObject();
    }
}
