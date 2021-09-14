using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.JsonConverters
{
    /// <summary>
    /// Converts <see cref="ResourceObject" /> to/from JSON.
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class ResourceObjectConverter : JsonConverter<ResourceObject>
    {
        private static readonly JsonEncodedText TypeText = JsonEncodedText.Encode("type");
        private static readonly JsonEncodedText IdText = JsonEncodedText.Encode("id");
        private static readonly JsonEncodedText LidText = JsonEncodedText.Encode("lid");
        private static readonly JsonEncodedText MetaText = JsonEncodedText.Encode("meta");
        private static readonly JsonEncodedText AttributesText = JsonEncodedText.Encode("attributes");
        private static readonly JsonEncodedText RelationshipsText = JsonEncodedText.Encode("relationships");
        private static readonly JsonEncodedText LinksText = JsonEncodedText.Encode("links");

        private readonly IResourceGraph _resourceGraph;

        public ResourceObjectConverter(IResourceGraph resourceGraph)
        {
            _resourceGraph = resourceGraph;
        }

        /// <summary>
        /// Resolves the resource type and attributes against the resource graph. Because attribute values in <see cref="ResourceObject" /> are typed as
        /// <see cref="object" />, we must lookup and supply the target type to the serializer.
        /// </summary>
        public override ResourceObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Inside a JsonConverter there is no way to know where in the JSON object tree we are. And the serializer is unable to provide
            // the correct position either. So we avoid an exception on missing/invalid 'type' element and postpone producing an error response
            // to the post-processing phase.

            var resourceObject = new ResourceObject
            {
                // The 'attributes' element may occur before 'type', but we need to know the resource type before we can deserialize attributes
                // into their corresponding CLR types.
                Type = TryPeekType(ref reader)
            };

            ResourceContext resourceContext = resourceObject.Type != null ? _resourceGraph.TryGetResourceContext(resourceObject.Type) : null;

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.EndObject:
                    {
                        return resourceObject;
                    }
                    case JsonTokenType.PropertyName:
                    {
                        string propertyName = reader.GetString();
                        reader.Read();

                        switch (propertyName)
                        {
                            case "id":
                            {
                                if (reader.TokenType != JsonTokenType.String)
                                {
                                    // Newtonsoft.Json used to auto-convert number to strings, while System.Text.Json does not. This is so likely
                                    // to hit users during upgrade that we special-case for this and produce a helpful error message.
                                    var jsonElement = JsonConverterSupport.ReadSubTree<JsonElement>(ref reader, options);
                                    throw new JsonException($"Failed to convert ID '{jsonElement}' of type '{jsonElement.ValueKind}' to type 'String'.");
                                }

                                resourceObject.Id = reader.GetString();
                                break;
                            }
                            case "lid":
                            {
                                resourceObject.Lid = reader.GetString();
                                break;
                            }
                            case "attributes":
                            {
                                if (resourceContext != null)
                                {
                                    resourceObject.Attributes = ReadAttributes(ref reader, options, resourceContext);
                                }
                                else
                                {
                                    reader.Skip();
                                }

                                break;
                            }
                            case "relationships":
                            {
                                resourceObject.Relationships = JsonConverterSupport.ReadSubTree<IDictionary<string, RelationshipObject>>(ref reader, options);
                                break;
                            }
                            case "links":
                            {
                                resourceObject.Links = JsonConverterSupport.ReadSubTree<ResourceLinks>(ref reader, options);
                                break;
                            }
                            case "meta":
                            {
                                resourceObject.Meta = JsonConverterSupport.ReadSubTree<IDictionary<string, object>>(ref reader, options);
                                break;
                            }
                            default:
                            {
                                reader.Skip();
                                break;
                            }
                        }

                        break;
                    }
                }
            }

            throw JsonConverterSupport.GetEndOfStreamError();
        }

        private static string TryPeekType(ref Utf8JsonReader reader)
        {
            // https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to?pivots=dotnet-5-0#an-alternative-way-to-do-polymorphic-deserialization
            Utf8JsonReader readerClone = reader;

            while (readerClone.Read())
            {
                if (readerClone.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = readerClone.GetString();
                    readerClone.Read();

                    switch (propertyName)
                    {
                        case "type":
                        {
                            return readerClone.GetString();
                        }
                        default:
                        {
                            readerClone.Skip();
                            break;
                        }
                    }
                }
            }

            return null;
        }

        private static IDictionary<string, object> ReadAttributes(ref Utf8JsonReader reader, JsonSerializerOptions options, ResourceContext resourceContext)
        {
            var attributes = new Dictionary<string, object>();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.EndObject:
                    {
                        return attributes;
                    }
                    case JsonTokenType.PropertyName:
                    {
                        string attributeName = reader.GetString();
                        reader.Read();

                        AttrAttribute attribute = resourceContext.TryGetAttributeByPublicName(attributeName);
                        PropertyInfo property = attribute?.Property;

                        if (property != null)
                        {
                            object attributeValue;

                            if (property.Name == nameof(Identifiable.Id))
                            {
                                attributeValue = JsonInvalidAttributeInfo.Id;
                            }
                            else
                            {
                                try
                                {
                                    attributeValue = JsonSerializer.Deserialize(ref reader, property.PropertyType, options);
                                }
                                catch (JsonException)
                                {
                                    // Inside a JsonConverter there is no way to know where in the JSON object tree we are. And the serializer
                                    // is unable to provide the correct position either. So we avoid an exception and postpone producing an error
                                    // response to the post-processing phase, by setting a sentinel value.
                                    var jsonElement = JsonConverterSupport.ReadSubTree<JsonElement>(ref reader, options);

                                    attributeValue = new JsonInvalidAttributeInfo(attributeName, property.PropertyType, jsonElement.ToString(),
                                        jsonElement.ValueKind);
                                }
                            }

                            attributes.Add(attributeName!, attributeValue);
                        }
                        else
                        {
                            reader.Skip();
                        }

                        break;
                    }
                }
            }

            throw JsonConverterSupport.GetEndOfStreamError();
        }

        /// <summary>
        /// Ensures that attribute values are not wrapped in <see cref="JsonElement" />s.
        /// </summary>
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

            if (!value.Attributes.IsNullOrEmpty())
            {
                writer.WritePropertyName(AttributesText);
                JsonConverterSupport.WriteSubTree(writer, value.Attributes, options);
            }

            if (!value.Relationships.IsNullOrEmpty())
            {
                writer.WritePropertyName(RelationshipsText);
                JsonConverterSupport.WriteSubTree(writer, value.Relationships, options);
            }

            if (value.Links != null && value.Links.HasValue())
            {
                writer.WritePropertyName(LinksText);
                JsonConverterSupport.WriteSubTree(writer, value.Links, options);
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
