using System.Collections;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters;

/// <inheritdoc cref="IRelationshipDataAdapter" />
public sealed class RelationshipDataAdapter : BaseAdapter, IRelationshipDataAdapter
{
    private readonly IResourceIdentifierObjectAdapter _resourceIdentifierObjectAdapter;

    public RelationshipDataAdapter(IResourceIdentifierObjectAdapter resourceIdentifierObjectAdapter)
    {
        ArgumentNullException.ThrowIfNull(resourceIdentifierObjectAdapter);

        _resourceIdentifierObjectAdapter = resourceIdentifierObjectAdapter;
    }

    /// <inheritdoc />
    public object? Convert(SingleOrManyData<ResourceObject> data, RelationshipAttribute relationship, bool useToManyElementType, RequestAdapterState state)
    {
        SingleOrManyData<ResourceIdentifierObject> identifierData = ToIdentifierData(data);
        return Convert(identifierData, relationship, useToManyElementType, state);
    }

    private static SingleOrManyData<ResourceIdentifierObject> ToIdentifierData(SingleOrManyData<ResourceObject> data)
    {
        if (!data.IsAssigned)
        {
            return default;
        }

        object? newValue = null;

        if (data.ManyValue != null)
        {
            newValue = data.ManyValue.Select(resourceObject => new ResourceIdentifierObject
            {
                Type = resourceObject.Type,
                Id = resourceObject.Id,
                Lid = resourceObject.Lid
            });
        }
        else if (data.SingleValue != null)
        {
            newValue = new ResourceIdentifierObject
            {
                Type = data.SingleValue.Type,
                Id = data.SingleValue.Id,
                Lid = data.SingleValue.Lid
            };
        }

        return new SingleOrManyData<ResourceIdentifierObject>(newValue);
    }

    /// <inheritdoc />
    public object? Convert(SingleOrManyData<ResourceIdentifierObject> data, RelationshipAttribute relationship, bool useToManyElementType,
        RequestAdapterState state)
    {
        ArgumentNullException.ThrowIfNull(relationship);
        ArgumentNullException.ThrowIfNull(state);
        AssertHasData(data, state);

        using IDisposable _ = state.Position.PushElement("data");

        var requirements = new ResourceIdentityRequirements
        {
            ResourceType = relationship.RightType,
            EvaluateIdConstraint = _ => JsonElementConstraint.Required,
            EvaluateAllowLid = _ => state.Request.Kind == EndpointKind.AtomicOperations,
            RelationshipName = relationship.PublicName
        };

        return relationship is HasOneAttribute
            ? ConvertToOneRelationshipData(data, requirements, state)
            : ConvertToManyRelationshipData(data, relationship, requirements, useToManyElementType, state);
    }

    private IIdentifiable? ConvertToOneRelationshipData(SingleOrManyData<ResourceIdentifierObject> data, ResourceIdentityRequirements requirements,
        RequestAdapterState state)
    {
        AssertDataHasSingleValue(data, true, state);

        return data.SingleValue != null ? _resourceIdentifierObjectAdapter.Convert(data.SingleValue, requirements, state) : null;
    }

    private IEnumerable ConvertToManyRelationshipData(SingleOrManyData<ResourceIdentifierObject> data, RelationshipAttribute relationship,
        ResourceIdentityRequirements requirements, bool useToManyElementType, RequestAdapterState state)
    {
        AssertDataHasManyValue(data, state);

        List<IIdentifiable> rightResources = [];

        foreach (ResourceIdentifierObject resourceIdentifierObject in data.ManyValue!)
        {
            using IDisposable _ = state.Position.PushArrayIndex(rightResources.Count);

            IIdentifiable rightResource = _resourceIdentifierObjectAdapter.Convert(resourceIdentifierObject, requirements, state);
            rightResources.Add(rightResource);
        }

        if (useToManyElementType)
        {
            return CollectionConverter.Instance.CopyToTypedCollection(rightResources, relationship.Property.PropertyType);
        }

        var resourceSet = new HashSet<IIdentifiable>(IdentifiableComparer.Instance);
        resourceSet.UnionWith(rightResources);
        return resourceSet;
    }
}
