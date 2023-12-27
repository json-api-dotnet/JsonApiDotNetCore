using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters;

/// <inheritdoc cref="IAtomicReferenceAdapter" />
[PublicAPI]
public sealed class AtomicReferenceAdapter(IResourceGraph resourceGraph, IResourceFactory resourceFactory)
    : ResourceIdentityAdapter(resourceGraph, resourceFactory), IAtomicReferenceAdapter
{
    /// <inheritdoc />
    public AtomicReferenceResult Convert(AtomicReference atomicReference, ResourceIdentityRequirements requirements, RequestAdapterState state)
    {
        ArgumentGuard.NotNull(atomicReference);
        ArgumentGuard.NotNull(requirements);
        ArgumentGuard.NotNull(state);

        using IDisposable _ = state.Position.PushElement("ref");
        (IIdentifiable resource, ResourceType resourceType) = ConvertResourceIdentity(atomicReference, requirements, state);

        RelationshipAttribute? relationship = atomicReference.Relationship != null
            ? ConvertRelationship(atomicReference.Relationship, resourceType, state)
            : null;

        return new AtomicReferenceResult(resource, resourceType, relationship);
    }

    private RelationshipAttribute ConvertRelationship(string relationshipName, ResourceType resourceType, RequestAdapterState state)
    {
        using IDisposable _ = state.Position.PushElement("relationship");
        RelationshipAttribute? relationship = resourceType.FindRelationshipByPublicName(relationshipName);

        AssertIsKnownRelationship(relationship, relationshipName, resourceType, state);
        AssertToManyInAddOrRemoveRelationship(relationship, state);
        AssertRelationshipChangeNotBlocked(relationship, state);

        return relationship;
    }
}
