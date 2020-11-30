using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <inheritdoc />
    public interface ISetRelationshipProcessor<TResource> : ISetRelationshipProcessor<TResource, int>
        where TResource : class, IIdentifiable<int>
    {
    }

    /// <summary>
    /// Processes a single operation to perform a complete replacement of a relationship on an existing resource.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    /// <typeparam name="TId">The resource identifier type.</typeparam>
    public interface ISetRelationshipProcessor<TResource, TId> : IAtomicOperationProcessor
        where TResource : class, IIdentifiable<TId>
    {
    }
}
