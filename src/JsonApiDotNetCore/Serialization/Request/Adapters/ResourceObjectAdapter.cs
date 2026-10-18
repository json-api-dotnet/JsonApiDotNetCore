using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters;

/// <inheritdoc cref="IResourceObjectAdapter" />
public sealed class ResourceObjectAdapter : ResourceIdentityAdapter, IResourceObjectAdapter
{
    private readonly IJsonApiOptions _options;
    private readonly IRelationshipDataAdapter _relationshipDataAdapter;

    public ResourceObjectAdapter(IResourceGraph resourceGraph, IResourceFactory resourceFactory, IJsonApiOptions options,
        IRelationshipDataAdapter relationshipDataAdapter)
        : base(resourceGraph, resourceFactory)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(relationshipDataAdapter);

        _options = options;
        _relationshipDataAdapter = relationshipDataAdapter;
    }

    /// <inheritdoc />
    public (IIdentifiable resource, ResourceType resourceType) Convert(ResourceObject resourceObject, ResourceIdentityRequirements requirements,
        RequestAdapterState state)
    {
        ArgumentNullException.ThrowIfNull(resourceObject);
        ArgumentNullException.ThrowIfNull(requirements);
        ArgumentNullException.ThrowIfNull(state);

        (IIdentifiable resource, ResourceType resourceType) = ConvertResourceIdentity(resourceObject, requirements, state);

        ConvertAttributes(resourceObject.Attributes, resource, resourceType, state);
        ConvertRelationships(resourceObject.Relationships, resource, resourceType, state);

        return (resource, resourceType);
    }

    private void ConvertAttributes(IDictionary<string, object?>? resourceObjectAttributes, IIdentifiable resource, ResourceType resourceType,
        RequestAdapterState state)
    {
        using IDisposable _ = state.Position.PushElement("attributes");

        foreach ((string attributeName, object? attributeValue) in resourceObjectAttributes.EmptyIfNull())
        {
            ConvertAttribute(resource, attributeName, attributeValue, resourceType, state);
        }
    }

    private void ConvertAttribute(IIdentifiable resource, string attributeName, object? attributeValue, ResourceType resourceType, RequestAdapterState state)
    {
        using IDisposable _ = state.Position.PushElement(attributeName);
        AttrAttribute? attr = resourceType.FindAttributeByPublicName(attributeName);

        if (attr == null && _options.AllowUnknownFieldsInRequestBody)
        {
            return;
        }

        AssertIsKnownAttribute(attr, attributeName, resourceType, state);
        AssertNoInvalidAttribute(attributeValue, state);
        AssertSetAttributeInCreateResourceNotBlocked(attr, resourceType, state);
        AssertSetAttributeInUpdateResourceNotBlocked(attr, resourceType, state);
        AssertNotReadOnly(attr, resourceType, state);

        if (attr.IsCompound)
        {
            if (attributeValue is JsonElement jsonElement)
            {
                attributeValue = attr.IsCollection
                    ? ConvertCompoundCollection(jsonElement, attr.CompoundType!, attr.Property.PropertyType, attributeName, state)
                    : ConvertCompoundObject(jsonElement, attr.CompoundType!, attr.Property.PropertyType, attributeName, state);
            }
            else if (attributeValue != null && !attr.Property.PropertyType.IsInstanceOfType(attributeValue))
            {
                JsonElement serializedElement = JsonSerializer.SerializeToElement(attributeValue, _options.SerializerReadOptions);
                attributeValue = attr.IsCollection
                    ? ConvertCompoundCollection(serializedElement, attr.CompoundType!, attr.Property.PropertyType, attributeName, state)
                    : ConvertCompoundObject(serializedElement, attr.CompoundType!, attr.Property.PropertyType, attributeName, state);
            }
        }
        else if (attributeValue != null && !attr.Property.PropertyType.IsInstanceOfType(attributeValue))
        {
            try
            {
                if (attributeValue is JsonElement jsonElement)
                {
                    attributeValue = jsonElement.Deserialize(attr.Property.PropertyType, _options.SerializerReadOptions);
                }
                else if (attr.IsCollection)
                {
                    JsonElement serializedElement = JsonSerializer.SerializeToElement(attributeValue, _options.SerializerReadOptions);
                    attributeValue = serializedElement.Deserialize(attr.Property.PropertyType, _options.SerializerReadOptions);
                }
            }
            catch (JsonException exception)
            {
                string typeName = RuntimeTypeConverter.GetFriendlyTypeName(attr.Property.PropertyType);

                throw new ModelConversionException(state.Position, "Incompatible attribute value found.",
                    $"Failed to convert attribute '{attributeName}' with value '{attributeValue}' to type '{typeName}'.", innerException: exception);
            }
        }

        attr.SetValue(resource, attributeValue);
        state.WritableTargetedFields!.Attributes.Add(attr);
    }

    private object? ConvertCompoundObject(JsonElement element, CompoundType compoundType, Type targetType, string? memberName,
        RequestAdapterState state)
    {
        if (element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        if (element.ValueKind != JsonValueKind.Object)
        {
            string typeName = RuntimeTypeConverter.GetFriendlyTypeName(targetType);
            string message = memberName != null
                ? $"Failed to convert attribute '{memberName}' with value '{element}' of type '{element.ValueKind}' to type '{typeName}'."
                : $"Failed to convert element with value '{element}' of type '{element.ValueKind}' to type '{typeName}'.";

            throw new ModelConversionException(state.Position, "Incompatible attribute value found.", message);
        }

        object instance = CreateCompoundInstance(targetType);

        foreach (JsonProperty jsonProperty in element.EnumerateObject())
        {
            string childMemberName = jsonProperty.Name;
            AttrAttribute? childAttr = compoundType.FindAttributeByPublicName(childMemberName);

            if (childAttr == null)
            {
                if (_options.AllowUnknownFieldsInRequestBody)
                {
                    continue;
                }

                using (state.Position.PushElement(childMemberName))
                {
                    AssertIsKnownAttribute(childAttr, childMemberName, compoundType, state);
                }
            }

            using (state.Position.PushElement(childAttr.PublicName))
            {
                AssertNotReadOnly(childAttr, compoundType, state);
                AssertSetAttributeInCreateResourceNotBlocked(childAttr, compoundType, state);
                AssertSetAttributeInUpdateResourceNotBlocked(childAttr, compoundType, state);

                object? childValue;

                if (childAttr.IsCompound)
                {
                    childValue = childAttr.IsCollection
                        ? ConvertCompoundCollection(jsonProperty.Value, childAttr.CompoundType!, childAttr.Property.PropertyType, childAttr.PublicName, state)
                        : ConvertCompoundObject(jsonProperty.Value, childAttr.CompoundType!, childAttr.Property.PropertyType, childAttr.PublicName, state);
                }
                else
                {
                    try
                    {
                        childValue = jsonProperty.Value.Deserialize(childAttr.Property.PropertyType, _options.SerializerReadOptions);
                    }
                    catch (JsonException exception)
                    {
                        string typeName = RuntimeTypeConverter.GetFriendlyTypeName(childAttr.Property.PropertyType);

                        throw new ModelConversionException(state.Position, "Incompatible attribute value found.",
                            $"Failed to convert attribute '{childAttr.PublicName}' with value '{jsonProperty.Value}' of type '{jsonProperty.Value.ValueKind}' to type '{typeName}'.",
                            innerException: exception);
                    }
                }

                childAttr.SetValue(instance, childValue);
            }
        }

        return instance;
    }

    private object? ConvertCompoundCollection(JsonElement element, CompoundType elementCompoundType, Type collectionType, string? memberName,
        RequestAdapterState state)
    {
        if (element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        if (element.ValueKind != JsonValueKind.Array)
        {
            string typeName = RuntimeTypeConverter.GetFriendlyTypeName(collectionType);
            string message = memberName != null
                ? $"Failed to convert attribute '{memberName}' with value '{element}' of type '{element.ValueKind}' to type '{typeName}'."
                : $"Failed to convert element with value '{element}' of type '{element.ValueKind}' to type '{typeName}'.";

            throw new ModelConversionException(state.Position, "Incompatible attribute value found.", message);
        }

        Type elementType = CollectionConverter.Instance.FindCollectionElementType(collectionType)
            ?? (collectionType.IsArray ? collectionType.GetElementType() : null)
            ?? elementCompoundType.ClrType;

        var items = new List<object?>();
        int index = 0;

        foreach (JsonElement itemElement in element.EnumerateArray())
        {
            using (state.Position.PushArrayIndex(index))
            {
                object? itemValue = itemElement.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
                    ? null
                    : ConvertCompoundObject(itemElement, elementCompoundType, elementType, null, state);

                items.Add(itemValue);
            }

            index++;
        }

        if (collectionType.IsArray)
        {
            Array array = Array.CreateInstance(elementType, items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                array.SetValue(items[i], i);
            }

            return array;
        }

        return CollectionConverter.Instance.CopyToTypedCollection(items, collectionType);
    }

    private static object CreateCompoundInstance(Type type)
    {
        try
        {
            return Activator.CreateInstance(type, nonPublic: true)
                ?? throw new InvalidOperationException($"Failed to instantiate compound type '{type.Name}'.");
        }
        catch (MissingMethodException exception)
        {
            throw new InvalidOperationException(
                $"Compound type '{type.Name}' must have a parameterless constructor (can be non-public) to support deserialization.", exception);
        }
    }

    private static void AssertIsKnownAttribute([NotNull] AttrAttribute? attr, string attributeName, IAttributeContainer container, RequestAdapterState state)
    {
        if (attr == null)
        {
            string containerKind = container is ResourceType ? "resource type" : "compound type";
            throw new ModelConversionException(state.Position, "Unknown attribute found.",
                $"Attribute '{attributeName}' does not exist on {containerKind} '{container}'.");
        }
    }

    private static void AssertNoInvalidAttribute(object? attributeValue, RequestAdapterState state)
    {
        if (attributeValue is JsonInvalidAttributeInfo info)
        {
            if (info == JsonInvalidAttributeInfo.Id)
            {
                throw new ModelConversionException(state.Position, "Resource ID is read-only.", null, innerException: info.InnerException);
            }

            string typeName = RuntimeTypeConverter.GetFriendlyTypeName(info.AttributeType);

            throw new ModelConversionException(state.Position, "Incompatible attribute value found.",
                $"Failed to convert attribute '{info.AttributeName}' with value '{info.JsonValue}' of type '{info.JsonType}' to type '{typeName}'.",
                innerException: info.InnerException);
        }
    }

    private static void AssertSetAttributeInCreateResourceNotBlocked(AttrAttribute attr, IAttributeContainer container, RequestAdapterState state)
    {
        if (state.Request.WriteOperation == WriteOperationKind.CreateResource && !attr.Capabilities.HasFlag(AttrCapabilities.AllowCreate))
        {
            string containerKind = container is ResourceType ? "resource type" : "compound type";
            throw new ModelConversionException(state.Position, "Attribute value cannot be assigned when creating resource.",
                $"The attribute '{attr}' on {containerKind} '{container}' cannot be assigned to.");
        }
    }

    private static void AssertSetAttributeInUpdateResourceNotBlocked(AttrAttribute attr, IAttributeContainer container, RequestAdapterState state)
    {
        if (state.Request.WriteOperation == WriteOperationKind.UpdateResource && !attr.Capabilities.HasFlag(AttrCapabilities.AllowChange))
        {
            string containerKind = container is ResourceType ? "resource type" : "compound type";
            throw new ModelConversionException(state.Position, "Attribute value cannot be assigned when updating resource.",
                $"The attribute '{attr}' on {containerKind} '{container}' cannot be assigned to.");
        }
    }

    private static void AssertNotReadOnly(AttrAttribute attr, IAttributeContainer container, RequestAdapterState state)
    {
        if (attr.Property.SetMethod == null)
        {
            string containerKind = container is ResourceType ? "resource type" : "compound type";
            throw new ModelConversionException(state.Position, "Attribute is read-only.",
                $"Attribute '{attr}' on {containerKind} '{container}' is read-only.");
        }
    }

    private void ConvertRelationships(IDictionary<string, RelationshipObject?>? resourceObjectRelationships, IIdentifiable resource, ResourceType resourceType,
        RequestAdapterState state)
    {
        using IDisposable _ = state.Position.PushElement("relationships");

        foreach ((string relationshipName, RelationshipObject? relationshipObject) in resourceObjectRelationships.EmptyIfNull())
        {
            ConvertRelationship(relationshipName, relationshipObject, resource, resourceType, state);
        }
    }

    private void ConvertRelationship(string relationshipName, RelationshipObject? relationshipObject, IIdentifiable resource, ResourceType resourceType,
        RequestAdapterState state)
    {
        using IDisposable _ = state.Position.PushElement(relationshipName);
        AssertObjectIsNotNull(relationshipObject, state);

        RelationshipAttribute? relationship = resourceType.FindRelationshipByPublicName(relationshipName);

        if (relationship == null && _options.AllowUnknownFieldsInRequestBody)
        {
            return;
        }

        AssertIsKnownRelationship(relationship, relationshipName, resourceType, state);
        AssertRelationshipChangeNotBlocked(relationship, state);

        object? rightValue = _relationshipDataAdapter.Convert(relationshipObject.Data, relationship, true, state);

        relationship.SetValue(resource, rightValue);
        state.WritableTargetedFields!.Relationships.Add(relationship);
    }
}
