using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.RequestAdapters
{
    /// <inheritdoc cref="IAtomicReferenceAdapter" />
    public sealed class AtomicReferenceAdapter : ResourceIdentityAdapter, IAtomicReferenceAdapter
    {
        public AtomicReferenceAdapter(IResourceGraph resourceGraph, IResourceFactory resourceFactory)
            : base(resourceGraph, resourceFactory)
        {
        }

        /// <inheritdoc />
        public AtomicReferenceResult Convert(AtomicReference atomicReference, ResourceIdentityRequirements requirements, RequestAdapterState state)
        {
            ArgumentGuard.NotNull(atomicReference, nameof(atomicReference));
            ArgumentGuard.NotNull(requirements, nameof(requirements));
            ArgumentGuard.NotNull(state, nameof(state));

            using IDisposable _ = state.Position.PushElement("ref");

            (IIdentifiable resource, ResourceContext resourceContext) = ConvertResourceIdentity(atomicReference, requirements, state);

            RelationshipAttribute relationship = atomicReference.Relationship != null
                ? ConvertRelationship(atomicReference.Relationship, resourceContext, state)
                : null;

            return new AtomicReferenceResult(resource, resourceContext, relationship);
        }

        private RelationshipAttribute ConvertRelationship(string relationshipName, ResourceContext resourceContext, RequestAdapterState state)
        {
            using IDisposable _ = state.Position.PushElement("relationship");

            RelationshipAttribute relationship = resourceContext.TryGetRelationshipByPublicName(relationshipName);

            AssertIsKnownRelationship(relationship, relationshipName, resourceContext, state);
            AssertToManyInAddOrRemoveRelationship(relationship, state);

            return relationship;
        }
    }
}
