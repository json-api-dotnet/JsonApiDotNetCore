using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters
{
    /// <inheritdoc />
    public sealed class AtomicOperationObjectAdapter : IAtomicOperationObjectAdapter
    {
        private readonly IResourceDataInOperationsRequestAdapter _resourceDataInOperationsRequestAdapter;
        private readonly IAtomicReferenceAdapter _atomicReferenceAdapter;
        private readonly IRelationshipDataAdapter _relationshipDataAdapter;
        private readonly IJsonApiOptions _options;

        public AtomicOperationObjectAdapter(IJsonApiOptions options, IAtomicReferenceAdapter atomicReferenceAdapter,
            IResourceDataInOperationsRequestAdapter resourceDataInOperationsRequestAdapter, IRelationshipDataAdapter relationshipDataAdapter)
        {
            ArgumentGuard.NotNull(options, nameof(options));
            ArgumentGuard.NotNull(atomicReferenceAdapter, nameof(atomicReferenceAdapter));
            ArgumentGuard.NotNull(resourceDataInOperationsRequestAdapter, nameof(resourceDataInOperationsRequestAdapter));
            ArgumentGuard.NotNull(relationshipDataAdapter, nameof(relationshipDataAdapter));

            _options = options;
            _atomicReferenceAdapter = atomicReferenceAdapter;
            _resourceDataInOperationsRequestAdapter = resourceDataInOperationsRequestAdapter;
            _relationshipDataAdapter = relationshipDataAdapter;
        }

        /// <inheritdoc />
        public OperationContainer Convert(AtomicOperationObject atomicOperationObject, RequestAdapterState state)
        {
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
            ResourceIdentityRequirements requirements = CreateIdentityRequirements(state);
            IIdentifiable? primaryResource = null;

            AtomicReferenceResult? refResult = atomicOperationObject.Ref != null
                ? _atomicReferenceAdapter.Convert(atomicOperationObject.Ref, requirements, state)
                : null;

            if (refResult != null)
            {
                requirements = new ResourceIdentityRequirements
                {
                    ResourceType = refResult.ResourceType,
                    IdConstraint = requirements.IdConstraint,
                    IdValue = refResult.Resource.StringId,
                    LidValue = refResult.Resource.LocalId,
                    RelationshipName = refResult.Relationship?.PublicName
                };

                state.WritableRequest!.PrimaryId = refResult.Resource.StringId;
                state.WritableRequest.PrimaryResourceType = refResult.ResourceType;
                state.WritableRequest.Relationship = refResult.Relationship;
                state.WritableRequest.IsCollection = refResult.Relationship is HasManyAttribute;

                ConvertRefRelationship(atomicOperationObject.Data, refResult, state);

                primaryResource = refResult.Resource;
            }

            return (requirements, primaryResource);
        }

        private ResourceIdentityRequirements CreateIdentityRequirements(RequestAdapterState state)
        {
            JsonElementConstraint? idConstraint = state.Request.WriteOperation == WriteOperationKind.CreateResource
                ? _options.AllowClientGeneratedIds ? null : JsonElementConstraint.Forbidden
                : JsonElementConstraint.Required;

            return new ResourceIdentityRequirements
            {
                IdConstraint = idConstraint
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
}
