using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
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

        private static void AssertToManyInAddOrRemoveRelationship(RelationshipAttribute relationship, RequestAdapterState state)
        {
            bool requireToManyRelationship = state.Request.WriteOperation == WriteOperationKind.AddToRelationship ||
                state.Request.WriteOperation == WriteOperationKind.RemoveFromRelationship;

            if (requireToManyRelationship && relationship is not HasManyAttribute)
            {
                throw new DeserializationException(state.Position, "Only to-many relationships can be targeted through this operation.",
                    $"Relationship '{relationship.PublicName}' must be a to-many relationship.");
            }
        }
    }
}
