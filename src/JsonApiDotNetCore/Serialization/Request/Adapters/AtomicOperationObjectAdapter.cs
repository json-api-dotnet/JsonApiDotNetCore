using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters;

/// <inheritdoc cref="IAtomicOperationObjectAdapter" />
public sealed class AtomicOperationObjectAdapter : IAtomicOperationObjectAdapter
{
    private readonly IResourceDataInOperationsRequestAdapter _resourceDataInOperationsRequestAdapter;
    private readonly IAtomicReferenceAdapter _atomicReferenceAdapter;
    private readonly IRelationshipDataAdapter _relationshipDataAdapter;
    private readonly IJsonApiOptions _options;

    public AtomicOperationObjectAdapter(IJsonApiOptions options, IAtomicReferenceAdapter atomicReferenceAdapter,
        IResourceDataInOperationsRequestAdapter resourceDataInOperationsRequestAdapter, IRelationshipDataAdapter relationshipDataAdapter)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(atomicReferenceAdapter);
        ArgumentNullException.ThrowIfNull(resourceDataInOperationsRequestAdapter);
        ArgumentNullException.ThrowIfNull(relationshipDataAdapter);

        _options = options;
        _atomicReferenceAdapter = atomicReferenceAdapter;
        _resourceDataInOperationsRequestAdapter = resourceDataInOperationsRequestAdapter;
        _relationshipDataAdapter = relationshipDataAdapter;
    }

    /// <inheritdoc />
    public OperationContainer Convert(AtomicOperationObject atomicOperationObject, RequestAdapterState state)
    {
        ArgumentNullException.ThrowIfNull(atomicOperationObject);
        ArgumentNullException.ThrowIfNull(state);

        AssertNoHref(atomicOperationObject, state);

        WriteOperationKind writeOperation = ConvertOperationCode(atomicOperationObject, state);

        state.WritableTargetedFields = new TargetedFields();

        state.WritableRequest = new JsonApiRequest
        {
            Kind = EndpointKind.AtomicOperations,
            WriteOperation = writeOperation
        };

        (ResourceIdentityRequirements requirements, IIdentifiable? primaryResource) = ConvertRef(atomicOperationObject, state);

        if (writeOperation is WriteOperationKind.CreateResource or WriteOperationKind.UpdateResource)
        {
            primaryResource = _resourceDataInOperationsRequestAdapter.Convert(atomicOperationObject.Data, requirements, state);
        }

        return new OperationContainer(primaryResource!, state.WritableTargetedFields, state.Request);
    }

    private static void AssertNoHref(AtomicOperationObject atomicOperationObject, RequestAdapterState state)
    {
        if (atomicOperationObject.Href != null)
        {
            using IDisposable _ = state.Position.PushElement("href");
            throw new ModelConversionException(state.Position, "The 'href' element is not supported.", null);
        }
    }

    private WriteOperationKind ConvertOperationCode(AtomicOperationObject atomicOperationObject, RequestAdapterState state)
    {
        switch (atomicOperationObject.Code)
        {
            case AtomicOperationCode.Add:
            {
                if (atomicOperationObject.Ref is { Relationship: null })
                {
                    using IDisposable _ = state.Position.PushElement("ref");
                    throw new ModelConversionException(state.Position, "The 'relationship' element is required.", null);
                }

                return atomicOperationObject.Ref == null ? WriteOperationKind.CreateResource : WriteOperationKind.AddToRelationship;
            }
            case AtomicOperationCode.Update:
            {
                return atomicOperationObject.Ref?.Relationship != null ? WriteOperationKind.SetRelationship : WriteOperationKind.UpdateResource;
            }
            case AtomicOperationCode.Remove:
            {
                if (atomicOperationObject.Ref == null)
                {
                    throw new ModelConversionException(state.Position, "The 'ref' element is required.", null);
                }

                return atomicOperationObject.Ref.Relationship != null ? WriteOperationKind.RemoveFromRelationship : WriteOperationKind.DeleteResource;
            }
        }

        throw new NotSupportedException($"Unknown operation code '{atomicOperationObject.Code}'.");
    }

    private (ResourceIdentityRequirements requirements, IIdentifiable? primaryResource) ConvertRef(AtomicOperationObject atomicOperationObject,
        RequestAdapterState state)
    {
        ResourceIdentityRequirements requirements = CreateRefRequirements(state);
        IIdentifiable? primaryResource = null;

        AtomicReferenceResult? refResult = atomicOperationObject.Ref != null
            ? _atomicReferenceAdapter.Convert(atomicOperationObject.Ref, requirements, state)
            : null;

        if (refResult != null)
        {
            state.WritableRequest!.PrimaryId = refResult.Resource.StringId;
            state.WritableRequest.PrimaryResourceType = refResult.ResourceType;
            state.WritableRequest.Relationship = refResult.Relationship;
            state.WritableRequest.IsCollection = refResult.Relationship is HasManyAttribute;

            ConvertRefRelationship(atomicOperationObject.Data, refResult, state);

            requirements = CreateDataRequirements(refResult, requirements);
            primaryResource = refResult.Resource;
        }

        return (requirements, primaryResource);
    }

    private ResourceIdentityRequirements CreateRefRequirements(RequestAdapterState state)
    {
        return new ResourceIdentityRequirements
        {
            EvaluateIdConstraint = resourceType =>
                ResourceIdentityRequirements.DoEvaluateIdConstraint(resourceType, state.Request.WriteOperation, _options.ClientIdGeneration),
            EvaluateAllowLid = resourceType =>
                ResourceIdentityRequirements.DoEvaluateAllowLid(resourceType, state.Request.WriteOperation, _options.ClientIdGeneration)
        };
    }

    private static ResourceIdentityRequirements CreateDataRequirements(AtomicReferenceResult refResult, ResourceIdentityRequirements refRequirements)
    {
        return new ResourceIdentityRequirements
        {
            ResourceType = refResult.ResourceType,
            EvaluateIdConstraint = refRequirements.EvaluateIdConstraint,
            EvaluateAllowLid = refRequirements.EvaluateAllowLid,
            IdValue = refResult.Resource.StringId,
            LidValue = refResult.Resource.LocalId,
            RelationshipName = refResult.Relationship?.PublicName
        };
    }

    private void ConvertRefRelationship(SingleOrManyData<ResourceObject> relationshipData, AtomicReferenceResult refResult, RequestAdapterState state)
    {
        if (refResult.Relationship != null)
        {
            state.WritableRequest!.SecondaryResourceType = refResult.Relationship.RightType;

            state.WritableTargetedFields!.Relationships.Add(refResult.Relationship);

            object? rightValue = _relationshipDataAdapter.Convert(relationshipData, refResult.Relationship, true, state);
            refResult.Relationship.SetValue(refResult.Resource, rightValue);
        }
    }
}
