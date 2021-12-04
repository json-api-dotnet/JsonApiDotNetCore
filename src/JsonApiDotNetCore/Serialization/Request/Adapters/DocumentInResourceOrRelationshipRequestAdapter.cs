using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters;

/// <inheritdoc />
public sealed class DocumentInResourceOrRelationshipRequestAdapter : IDocumentInResourceOrRelationshipRequestAdapter
{
    private readonly IJsonApiOptions _options;
    private readonly IResourceDataAdapter _resourceDataAdapter;
    private readonly IRelationshipDataAdapter _relationshipDataAdapter;

    public DocumentInResourceOrRelationshipRequestAdapter(IJsonApiOptions options, IResourceDataAdapter resourceDataAdapter,
        IRelationshipDataAdapter relationshipDataAdapter)
    {
        ArgumentGuard.NotNull(options, nameof(options));
        ArgumentGuard.NotNull(resourceDataAdapter, nameof(resourceDataAdapter));
        ArgumentGuard.NotNull(relationshipDataAdapter, nameof(relationshipDataAdapter));

        _options = options;
        _resourceDataAdapter = resourceDataAdapter;
        _relationshipDataAdapter = relationshipDataAdapter;
    }

    /// <inheritdoc />
    public object? Convert(Document document, RequestAdapterState state)
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

                ResourceIdentityAdapter.AssertToManyInAddOrRemoveRelationship(state.Request.Relationship, state);

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
            ResourceType = state.Request.PrimaryResourceType,
            IdConstraint = idConstraint,
            IdValue = state.Request.PrimaryId
        };

        return requirements;
    }
}
