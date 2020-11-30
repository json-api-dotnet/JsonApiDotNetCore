using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <inheritdoc />
    public interface IAddToRelationshipProcessor<TResource> : IAddToRelationshipProcessor<TResource, int>
        where TResource : class, IIdentifiable<int>
    {
    }

    /// <summary>
    /// Processes a single operation to add resources to a to-many relationship.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    /// <typeparam name="TId">The resource identifier type.</typeparam>
    public interface IAddToRelationshipProcessor<TResource, TId> : IAtomicOperationProcessor
        where TResource : class, IIdentifiable<TId>
    {
    }
}
