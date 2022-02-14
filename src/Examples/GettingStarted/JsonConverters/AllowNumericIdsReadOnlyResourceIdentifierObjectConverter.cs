using System.Text.Json;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.JsonConverters;
using JsonApiDotNetCore.Serialization.Objects;

namespace GettingStarted.JsonConverters;

/// <summary>
/// Converts <see cref="ResourceIdentifierObject" /> from JSON, being tolerant on incoming numeric IDs.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class AllowNumericIdsReadOnlyResourceIdentifierObjectConverter : JsonObjectConverter<ResourceIdentifierObject>
{
    public override ResourceIdentifierObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var resourceIdentifierObject = new ResourceIdentifierObject();

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.EndObject:
                {
                    return resourceIdentifierObject;
                }
                case JsonTokenType.PropertyName:
                {
                    string? propertyName = reader.GetString();
                    reader.Read();

                    switch (propertyName)
                    {
                        case "type":
                        {
                            resourceIdentifierObject.Type = reader.GetString();
                            break;
                        }
                        case "id":
                        {
                            if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt64(out long value))
                            {
                                resourceIdentifierObject.Id = value.ToString();
                            }
                            else if (reader.TokenType != JsonTokenType.String)
                            {
                                // Newtonsoft.Json used to auto-convert number to strings, while System.Text.Json does not. This is so likely
                                // to hit users during upgrade that we special-case for this and produce a helpful error message.
                                var jsonElement = ReadSubTree<JsonElement>(ref reader, options);
                                throw new JsonException($"Failed to convert ID '{jsonElement}' of type '{jsonElement.ValueKind}' to type 'String'.");
                            }
                            else
                            {
                                resourceIdentifierObject.Id = reader.GetString();
                            }

                            break;
                        }
                        case "lid":
                        {
                            resourceIdentifierObject.Lid = reader.GetString();
                            break;
                        }
                        case "meta":
                        {
                            resourceIdentifierObject.Meta = ReadSubTree<IDictionary<string, object?>>(ref reader, options);
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

    public override void Write(Utf8JsonWriter writer, ResourceIdentifierObject value, JsonSerializerOptions options)
    {
        throw new NotSupportedException("This converter cannot be used for writing JSON.");
    }
}
