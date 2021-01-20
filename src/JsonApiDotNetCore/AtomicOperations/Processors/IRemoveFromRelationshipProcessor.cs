using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <inheritdoc />
    public interface IRemoveFromRelationshipProcessor<TResource> : IRemoveFromRelationshipProcessor<TResource, int>
        where TResource : class, IIdentifiable<int>
    {
    }

    /// <summary>
    /// Processes a single operation to remove resources from a to-many relationship.
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    /// <typeparam name="TId"></typeparam>
    public interface IRemoveFromRelationshipProcessor<TResource, TId> : IOperationProcessor
        where TResource : class, IIdentifiable<TId>
    {
    }
}
