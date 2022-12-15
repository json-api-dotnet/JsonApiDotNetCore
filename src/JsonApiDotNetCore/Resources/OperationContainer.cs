using JetBrains.Annotations;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Resources;

/// <summary>
/// Represents a write operation on a JSON:API resource.
/// </summary>
[PublicAPI]
public sealed class OperationContainer
{
    private static readonly CollectionConverter CollectionConverter = new();

    public IIdentifiable Resource { get; }
    public ITargetedFields TargetedFields { get; }
    public IJsonApiRequest Request { get; }

    public OperationContainer(IIdentifiable resource, ITargetedFields targetedFields, IJsonApiRequest request)
    {
        ArgumentGuard.NotNull(resource);
        ArgumentGuard.NotNull(targetedFields);
        ArgumentGuard.NotNull(request);

        Resource = resource;
        TargetedFields = targetedFields;
        Request = request;
    }

    public void SetTransactionId(string transactionId)
    {
        ((JsonApiRequest)Request).TransactionId = transactionId;
    }

    public OperationContainer WithResource(IIdentifiable resource)
    {
        ArgumentGuard.NotNull(resource);

        return new OperationContainer(resource, TargetedFields, Request);
    }

#pragma warning disable AV1130 // Return type in method signature should be an interface to an unchangeable collection
    public ISet<IIdentifiable> GetSecondaryResources()
#pragma warning restore AV1130 // Return type in method signature should be an interface to an unchangeable collection
    {
        var secondaryResources = new HashSet<IIdentifiable>(IdentifiableComparer.Instance);

        foreach (RelationshipAttribute relationship in TargetedFields.Relationships)
        {
            AddSecondaryResources(relationship, secondaryResources);
        }

        return secondaryResources;
    }

    private void AddSecondaryResources(RelationshipAttribute relationship, HashSet<IIdentifiable> secondaryResources)
    {
        object? rightValue = relationship.GetValue(Resource);
        IReadOnlyCollection<IIdentifiable> rightResources = CollectionConverter.ExtractResources(rightValue);

        secondaryResources.UnionWith(rightResources);
    }
}
