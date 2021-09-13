using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.JsonConverters
{
    /// <summary>
    /// Converts <see cref="Document" /> to JSON.
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class WriteOnlyDocumentConverter : JsonConverter<Document>
    {
        private static readonly JsonEncodedText JsonApiText = JsonEncodedText.Encode("jsonapi");
        private static readonly JsonEncodedText LinksText = JsonEncodedText.Encode("links");
        private static readonly JsonEncodedText DataText = JsonEncodedText.Encode("data");
        private static readonly JsonEncodedText AtomicOperationsText = JsonEncodedText.Encode("atomic:operations");
        private static readonly JsonEncodedText AtomicResultsText = JsonEncodedText.Encode("atomic:results");
        private static readonly JsonEncodedText ErrorsText = JsonEncodedText.Encode("errors");
        private static readonly JsonEncodedText IncludedText = JsonEncodedText.Encode("included");
        private static readonly JsonEncodedText MetaText = JsonEncodedText.Encode("meta");

        public override Document Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException("This converter cannot be used for reading JSON.");
        }

        /// <summary>
        /// Conditionally writes <code>"data": null</code> or omits it, depending on <see cref="SingleOrManyData{TObject}.IsAssigned" />.
        /// </summary>
        public override void Write(Utf8JsonWriter writer, Document value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            if (value.JsonApi != null)
            {
                writer.WritePropertyName(JsonApiText);
                JsonConverterSupport.WriteSubTree(writer, value.JsonApi, options);
            }

            if (value.Links != null && value.Links.HasValue())
            {
                writer.WritePropertyName(LinksText);
                JsonConverterSupport.WriteSubTree(writer, value.Links, options);
            }

            if (value.Data.IsAssigned)
            {
                writer.WritePropertyName(DataText);
                JsonConverterSupport.WriteSubTree(writer, value.Data, options);
            }

            if (!value.Operations.IsNullOrEmpty())
            {
                writer.WritePropertyName(AtomicOperationsText);
                JsonConverterSupport.WriteSubTree(writer, value.Operations, options);
            }

            if (!value.Results.IsNullOrEmpty())
            {
                writer.WritePropertyName(AtomicResultsText);
                JsonConverterSupport.WriteSubTree(writer, value.Results, options);
            }

            if (!value.Errors.IsNullOrEmpty())
            {
                writer.WritePropertyName(ErrorsText);
                JsonConverterSupport.WriteSubTree(writer, value.Errors, options);
            }

            if (value.Included != null)
            {
                writer.WritePropertyName(IncludedText);
                JsonConverterSupport.WriteSubTree(writer, value.Included, options);
            }

            if (!value.Meta.IsNullOrEmpty())
            {
                writer.WritePropertyName(MetaText);
                JsonConverterSupport.WriteSubTree(writer, value.Meta, options);
            }

            writer.WriteEndObject();
        }
    }
}
