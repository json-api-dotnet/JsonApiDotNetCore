using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Serialization.Request;

namespace JsonApiDotNetCore.Serialization.JsonConverters;

/// <summary>
/// Converts <see cref="ResourceObject" /> to/from JSON.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class ResourceObjectConverter : JsonObjectConverter<ResourceObject>
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
        ArgumentNullException.ThrowIfNull(resourceGraph);

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
            // The 'attributes' or 'relationships' element may occur before 'type', but we need to know the resource type
            // before we can deserialize attributes/relationships into their corresponding CLR types.
            Type = PeekType(reader)
        };

        ResourceType? resourceType = resourceObject.Type != null ? _resourceGraph.FindResourceType(resourceObject.Type) : null;

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
                    string? propertyName = reader.GetString();
                    reader.Read();

                    switch (propertyName)
                    {
                        case "id":
                        {
                            if (reader.TokenType != JsonTokenType.String)
                            {
                                // Newtonsoft.Json used to auto-convert number to strings, while System.Text.Json does not. This is so likely
                                // to hit users during upgrade that we special-case for this and produce a helpful error message.
                                var jsonElement = ReadSubTree<JsonElement>(ref reader, options);
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
                            if (resourceType != null)
                            {
                                resourceObject.Attributes = ReadAttributes(ref reader, options, resourceType);
                            }
                            else
                            {
                                reader.Skip();
                            }

                            break;
                        }
                        case "relationships":
                        {
                            if (resourceType != null)
                            {
                                resourceObject.Relationships = ReadRelationships(ref reader, options, resourceType);
                            }
                            else
                            {
                                reader.Skip();
                            }

                            break;
                        }
                        case "links":
                        {
                            resourceObject.Links = ReadSubTree<ResourceLinks>(ref reader, options);
                            break;
                        }
                        case "meta":
                        {
                            resourceObject.Meta = ReadSubTree<IDictionary<string, object?>>(ref reader, options);
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

        throw GetEndOfStreamError();
    }

    private static string? PeekType(Utf8JsonReader reader)
    {
        // This method receives a clone of the reader (which is a struct, and there's no ref modifier on the parameter),
        // so advancing here doesn't affect the reader position of the caller.

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string? propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case "type":
                    {
                        return reader.GetString();
                    }
                    default:
                    {
                        reader.Skip();
                        break;
                    }
                }
            }
        }

        return null;
    }

    private Dictionary<string, object?> ReadAttributes(ref Utf8JsonReader reader, JsonSerializerOptions options, ResourceType resourceType)
    {
        var attributes = new Dictionary<string, object?>();

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
                    string attributeName = reader.GetString() ?? string.Empty;
                    reader.Read();

                    int extensionSeparatorIndex = attributeName.IndexOf(':');

                    if (extensionSeparatorIndex != -1)
                    {
                        string extensionNamespace = attributeName[..extensionSeparatorIndex];
                        string extensionName = attributeName[(extensionSeparatorIndex + 1)..];

                        ValidateExtensionInAttributes(extensionNamespace, extensionName, resourceType, reader);
                        reader.Skip();
                        continue;
                    }

                    AttrAttribute? attribute = resourceType.FindAttributeByPublicName(attributeName);
                    PropertyInfo? property = attribute?.Property;

                    if (property != null)
                    {
                        object? attributeValue;

                        if (property.Name == nameof(Identifiable<object>.Id))
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
                                var jsonElement = ReadSubTree<JsonElement>(ref reader, options);

                                attributeValue = new JsonInvalidAttributeInfo(attributeName, property.PropertyType, jsonElement.ToString(),
                                    jsonElement.ValueKind);
                            }
                        }

                        attributes.Add(attributeName, attributeValue);
                    }
                    else
                    {
                        attributes.Add(attributeName, null);
                        reader.Skip();
                    }

                    break;
                }
            }
        }

        throw GetEndOfStreamError();
    }

    // Currently exposed for internal use only, so we don't need a breaking change when adding support for multiple extensions.
    // ReSharper disable once UnusedParameter.Global
    private protected virtual void ValidateExtensionInAttributes(string extensionNamespace, string extensionName, ResourceType resourceType,
        Utf8JsonReader reader)
    {
        throw new JsonException($"Unsupported usage of JSON:API extension '{extensionNamespace}' in attributes.");
    }

    private Dictionary<string, RelationshipObject?> ReadRelationships(ref Utf8JsonReader reader, JsonSerializerOptions options, ResourceType resourceType)
    {
        var relationships = new Dictionary<string, RelationshipObject?>();

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.EndObject:
                {
                    return relationships;
                }
                case JsonTokenType.PropertyName:
                {
                    string relationshipName = reader.GetString() ?? string.Empty;
                    reader.Read();

                    int extensionSeparatorIndex = relationshipName.IndexOf(':');

                    if (extensionSeparatorIndex != -1)
                    {
                        string extensionNamespace = relationshipName[..extensionSeparatorIndex];
                        string extensionName = relationshipName[(extensionSeparatorIndex + 1)..];

                        ValidateExtensionInRelationships(extensionNamespace, extensionName, resourceType, reader);
                        reader.Skip();
                        continue;
                    }

                    var relationshipObject = ReadSubTree<RelationshipObject?>(ref reader, options);
                    relationships[relationshipName] = relationshipObject;
                    break;
                }
            }
        }

        throw GetEndOfStreamError();
    }

    // Currently exposed for internal use only, so we don't need a breaking change when adding support for multiple extensions.
    // ReSharper disable once UnusedParameter.Global
    private protected virtual void ValidateExtensionInRelationships(string extensionNamespace, string extensionName, ResourceType resourceType,
        Utf8JsonReader reader)
    {
        throw new JsonException($"Unsupported usage of JSON:API extension '{extensionNamespace}' in relationships.");
    }

    /// <summary>
    /// Ensures that attribute values are not wrapped in <see cref="JsonElement" />s.
    /// </summary>
    public override void Write(Utf8JsonWriter writer, ResourceObject value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);

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
            writer.WriteStartObject();

            WriteExtensionInAttributes(writer, value);

            foreach ((string attributeName, object? attributeValue) in value.Attributes)
            {
                writer.WritePropertyName(attributeName);
                WriteSubTree(writer, attributeValue, options);
            }

            writer.WriteEndObject();
        }

        if (!value.Relationships.IsNullOrEmpty())
        {
            writer.WritePropertyName(RelationshipsText);
            writer.WriteStartObject();

            WriteExtensionInRelationships(writer, value);

            foreach ((string relationshipName, RelationshipObject? relationshipValue) in value.Relationships)
            {
                writer.WritePropertyName(relationshipName);
                WriteSubTree(writer, relationshipValue, options);
            }

            writer.WriteEndObject();
        }

        if (value.Links != null && value.Links.HasValue())
        {
            writer.WritePropertyName(LinksText);
            WriteSubTree(writer, value.Links, options);
        }

        if (!value.Meta.IsNullOrEmpty())
        {
            writer.WritePropertyName(MetaText);
            WriteSubTree(writer, value.Meta, options);
        }

        writer.WriteEndObject();
    }

    // Currently exposed for internal use only, so we don't need a breaking change when adding support for multiple extensions.
    private protected virtual void WriteExtensionInAttributes(Utf8JsonWriter writer, ResourceObject value)
    {
    }

    // Currently exposed for internal use only, so we don't need a breaking change when adding support for multiple extensions.
    private protected virtual void WriteExtensionInRelationships(Utf8JsonWriter writer, ResourceObject value)
    {
    }

    /// <summary>
    /// Throws a <see cref="JsonApiException" /> in such a way that <see cref="JsonApiReader" /> can reconstruct the source pointer.
    /// </summary>
    /// <param name="exception">
    /// The <see cref="JsonApiException" /> to throw, which may contain a relative source pointer.
    /// </param>
    [DoesNotReturn]
    [ContractAnnotation("=> halt")]
    private protected static void CapturedThrow(JsonApiException exception)
    {
        ExceptionDispatchInfo.SetCurrentStackTrace(exception);

        throw new NotSupportedException(null, exception)
        {
            Source = "System.Text.Json.Rethrowable"
        };
    }
}
