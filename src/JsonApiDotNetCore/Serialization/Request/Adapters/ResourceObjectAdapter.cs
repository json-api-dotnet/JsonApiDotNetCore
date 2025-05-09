using System.Collections;
using System.Diagnostics.CodeAnalysis;
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

        var container = new FieldContainer(resourceType, null);
        ConvertAttributes(resourceObject.Attributes, resource, container, state.WritableTargetedFields!.Attributes, state);
        ConvertRelationships(resourceObject.Relationships, resource, resourceType, state);

        return (resource, resourceType);
    }

    private void ConvertAttributes(IDictionary<string, object?>? attributeValues, object instance, FieldContainer container,
        HashSet<TargetedAttributeTree> targets, RequestAdapterState state)
    {
        using IDisposable _ = state.Position.PushElement("attributes");

        foreach ((string attributeName, object? attributeValue) in attributeValues.EmptyIfNull())
        {
            ConvertAttribute(attributeName, attributeValue, instance, container, targets, state);
        }
    }

    private void ConvertAttribute(string attributeName, object? attributeValue, object instance, FieldContainer container,
        HashSet<TargetedAttributeTree> targets, RequestAdapterState state)
    {
        using IDisposable _ = state.Position.PushElement(attributeName);
        AttrAttribute? attr = container.FindAttributeByPublicName(attributeName);

        if (attr == null && _options.AllowUnknownFieldsInRequestBody)
        {
            return;
        }

        AssertIsKnownAttribute(attr, attributeName, container, state);
        AssertNoInvalidAttribute(attributeValue, state);
        AssertSetAttributeInCreateResourceNotBlocked(attr, container, state);
        AssertSetAttributeInUpdateResourceNotBlocked(attr, container, state);
        AssertNotReadOnly(attr, container, state);

        if (attributeValue == null || attr.Kind == AttrKind.Primitive)
        {
            ConvertNullOrPrimitiveAttribute(attr, attributeValue, instance, targets);
        }
        else if (attr.Kind == AttrKind.Compound)
        {
            ConvertCompoundAttribute(attr, attributeValue, instance, targets, state);
        }
        else if (attr.Kind == AttrKind.CollectionOfPrimitive)
        {
            ConvertCollectionOfPrimitiveAttribute(attr, attributeValue, instance, targets);
        }
        else if (attr.Kind == AttrKind.CollectionOfCompound)
        {
            ConvertCollectionOfCompoundAttribute(attr, attributeValue, instance, targets, state);
        }
        else
        {
            throw new NotSupportedException($"Unknown attribute kind '{attr.Kind}'.");
        }
    }

    private static void ConvertNullOrPrimitiveAttribute(AttrAttribute attr, object? attributeValue, object instance, HashSet<TargetedAttributeTree> targets)
    {
        attr.SetValue(instance, attributeValue);

        var target = new TargetedAttributeTree(attr, []);
        targets.Add(target);
    }

    private void ConvertCompoundAttribute(AttrAttribute attr, object attributeValue, object instance, HashSet<TargetedAttributeTree> targets,
        RequestAdapterState state)
    {
        object subInstance = Activator.CreateInstance(attr.Property.PropertyType)!;
        attr.SetValue(instance, subInstance);

        var dictionary = (Dictionary<string, object?>)attributeValue;
        var subFieldContainer = new FieldContainer(null, attr);
        HashSet<TargetedAttributeTree> subTargets = [];

        ConvertAttributes(dictionary, subInstance, subFieldContainer, subTargets, state);

        var target = new TargetedAttributeTree(attr, subTargets);
        targets.Add(target);
    }

    private static void ConvertCollectionOfPrimitiveAttribute(AttrAttribute attr, object attributeValue, object instance,
        HashSet<TargetedAttributeTree> targets)
    {
        IEnumerable typedCollection = CollectionConverter.Instance.CopyToTypedCollection((IEnumerable)attributeValue, attr.Property.PropertyType);
        attr.SetValue(instance, typedCollection);

        var target = new TargetedAttributeTree(attr, []);
        targets.Add(target);
    }

    private void ConvertCollectionOfCompoundAttribute(AttrAttribute attr, object attributeValue, object instance, HashSet<TargetedAttributeTree> targets,
        RequestAdapterState state)
    {
        List<object?> subInstances = [];
        Type? elementType = CollectionConverter.Instance.FindCollectionElementType(attr.Property.PropertyType);

        if (elementType == null)
        {
            throw new ModelConversionException(state.Position, "TODO: Handle cases where array is sent instead of object, or object instead of array.", null);
        }

        foreach (IDictionary<string, object?>? subDictionary in (IEnumerable)attributeValue)
        {
            using IDisposable _ = state.Position.PushArrayIndex(subInstances.Count);

            object? subInstance = subDictionary == null ? null : Activator.CreateInstance(elementType);
            subInstances.Add(subInstance);

            if (subInstance != null)
            {
                var subFieldContainer = new FieldContainer(null, attr);
                ConvertAttributes(subDictionary, subInstance, subFieldContainer, [], state);
            }
        }

        IEnumerable subTypedCollection = CollectionConverter.Instance.CopyToTypedCollection(subInstances, attr.Property.PropertyType);
        attr.SetValue(instance, subTypedCollection);

        var target = new TargetedAttributeTree(attr, []);
        targets.Add(target);
    }

    private static void AssertIsKnownAttribute([NotNull] AttrAttribute? attr, string attributeName, FieldContainer container, RequestAdapterState state)
    {
        if (attr == null)
        {
            throw new ModelConversionException(state.Position, "Unknown attribute found.",
                $"Attribute '{attributeName}' does not exist on resource type '{resourceType.PublicName}'.");
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

    private static void AssertSetAttributeInCreateResourceNotBlocked(AttrAttribute attr, FieldContainer container, RequestAdapterState state)
    {
        if (state.Request.WriteOperation == WriteOperationKind.CreateResource && !attr.Capabilities.HasFlag(AttrCapabilities.AllowCreate))
        {
            throw new ModelConversionException(state.Position, "Attribute value cannot be assigned when creating resource.",
                container.Type != null
                    ? $"The attribute '{attr}' on resource type '{container.PublicName}' cannot be assigned to."
                    : $"The attribute '{attr}' on type '{container.PublicName}' cannot be assigned to.");
        }
    }

    private static void AssertSetAttributeInUpdateResourceNotBlocked(AttrAttribute attr, FieldContainer container, RequestAdapterState state)
    {
        if (state.Request.WriteOperation == WriteOperationKind.UpdateResource && !attr.Capabilities.HasFlag(AttrCapabilities.AllowChange))
        {
            throw new ModelConversionException(state.Position, "Attribute value cannot be assigned when updating resource.",
                container.Type != null
                    ? $"The attribute '{attr}' on resource type '{container.PublicName}' cannot be assigned to."
                    : $"The attribute '{attr}' on type '{container.PublicName}' cannot be assigned to.");
        }
    }

    private static void AssertNotReadOnly(AttrAttribute attr, FieldContainer container, RequestAdapterState state)
    {
        if (attr.Property.SetMethod == null)
        {
            throw new ModelConversionException(state.Position, "Attribute is read-only.",
                container.Type != null
                    ? $"Attribute '{attr}' on resource type '{container.PublicName}' is read-only."
                    : $"Attribute '{attr}' on type '{container.PublicName}' is read-only.");
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
