using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters;

/// <inheritdoc cref="IDocumentInResourceOrRelationshipRequestAdapter" />
public sealed class DocumentInResourceOrRelationshipRequestAdapter : IDocumentInResourceOrRelationshipRequestAdapter
{
    private readonly IJsonApiOptions _options;
    private readonly IResourceDataAdapter _resourceDataAdapter;
    private readonly IRelationshipDataAdapter _relationshipDataAdapter;

    public DocumentInResourceOrRelationshipRequestAdapter(IJsonApiOptions options, IResourceDataAdapter resourceDataAdapter,
        IRelationshipDataAdapter relationshipDataAdapter)
    {
        ArgumentGuard.NotNull(options);
        ArgumentGuard.NotNull(resourceDataAdapter);
        ArgumentGuard.NotNull(relationshipDataAdapter);

        _options = options;
        _resourceDataAdapter = resourceDataAdapter;
        _relationshipDataAdapter = relationshipDataAdapter;
    }

    /// <inheritdoc />
    public object? Convert(Document document, RequestAdapterState state)
    {
        ArgumentGuard.NotNull(document);
        ArgumentGuard.NotNull(state);

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
                ResourceIdentityAdapter.AssertRelationshipChangeNotBlocked(state.Request.Relationship, state);

                state.WritableTargetedFields.Relationships.Add(state.Request.Relationship);
                return _relationshipDataAdapter.Convert(document.Data, state.Request.Relationship, false, state);
            }
        }

        return null;
    }

    private ResourceIdentityRequirements CreateIdentityRequirements(RequestAdapterState state)
    {
        var requirements = new ResourceIdentityRequirements
        {
            ResourceType = state.Request.PrimaryResourceType,
            EvaluateIdConstraint = resourceType =>
                ResourceIdentityRequirements.DoEvaluateIdConstraint(resourceType, state.Request.WriteOperation, _options.ClientIdGeneration),
            IdValue = state.Request.PrimaryId
        };

        return requirements;
    }
}
