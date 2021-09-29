using System.Collections.Generic;
using System.Net;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.RequestAdapters
{
    /// <inheritdoc />
    public sealed class ResourceDocumentAdapter : IResourceDocumentAdapter
    {
        private readonly IJsonApiOptions _options;
        private readonly IResourceDataAdapter _resourceDataAdapter;
        private readonly IRelationshipDataAdapter _relationshipDataAdapter;

        public ResourceDocumentAdapter(IJsonApiOptions options, IResourceDataAdapter resourceDataAdapter, IRelationshipDataAdapter relationshipDataAdapter)
        {
            ArgumentGuard.NotNull(options, nameof(options));
            ArgumentGuard.NotNull(resourceDataAdapter, nameof(resourceDataAdapter));
            ArgumentGuard.NotNull(relationshipDataAdapter, nameof(relationshipDataAdapter));

            _options = options;
            _resourceDataAdapter = resourceDataAdapter;
            _relationshipDataAdapter = relationshipDataAdapter;
        }

        /// <inheritdoc />
        public object Convert(Document document, RequestAdapterState state)
        {
            state.WritableTargetedFields = new TargetedFields();

            switch (state.Request.WriteOperation)
            {
                case WriteOperationKind.CreateResource:
                case WriteOperationKind.UpdateResource:
                {
                    ResourceIdentityRequirements requirements = CreateIdentityRequirements(state);
                    return _resourceDataAdapter.Convert(document.Data, requirements, state);
                }
                case WriteOperationKind.SetRelationship:
                case WriteOperationKind.AddToRelationship:
                case WriteOperationKind.RemoveFromRelationship:
                {
                    if (state.Request.Relationship == null)
                    {
                        // Let the controller throw for unknown relationship, because it knows the relationship name that was used.
                        return new HashSet<IIdentifiable>(IdentifiableComparer.Instance);
                    }

                    AssertToManyInAddOrRemoveRelationship(state);

                    state.WritableTargetedFields.Relationships.Add(state.Request.Relationship);
                    return _relationshipDataAdapter.Convert(document.Data, state.Request.Relationship, false, state);
                }
            }

            return null;
        }

        private ResourceIdentityRequirements CreateIdentityRequirements(RequestAdapterState state)
        {
            JsonElementConstraint? idConstraint = state.Request.WriteOperation == WriteOperationKind.CreateResource
                ? _options.AllowClientGeneratedIds ? null : JsonElementConstraint.Forbidden
                : JsonElementConstraint.Required;

            var requirements = new ResourceIdentityRequirements
            {
                ResourceContext = state.Request.PrimaryResource,
                IdConstraint = idConstraint,
                IdValue = state.Request.PrimaryId
            };

            return requirements;
        }

        private static void AssertToManyInAddOrRemoveRelationship(RequestAdapterState state)
        {
            bool requireToManyRelationship = state.Request.WriteOperation == WriteOperationKind.AddToRelationship ||
                state.Request.WriteOperation == WriteOperationKind.RemoveFromRelationship;

            if (requireToManyRelationship && state.Request.Relationship is not HasManyAttribute)
            {
                throw new JsonApiException(new ErrorObject(HttpStatusCode.Forbidden)
                {
                    Title = "Only to-many relationships can be targeted through this endpoint.",
                    Detail = $"Relationship '{state.Request.Relationship.PublicName}' must be a to-many relationship."
                });
            }
        }
    }
}
