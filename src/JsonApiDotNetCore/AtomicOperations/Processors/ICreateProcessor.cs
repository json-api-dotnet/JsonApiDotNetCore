using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <inheritdoc />
    public interface ICreateProcessor<TResource> : ICreateProcessor<TResource, int>
        where TResource : class, IIdentifiable<int>
    {
    }

    /// <summary>
    /// Processes a single operation to create a new resource with attributes, relationships or both.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    /// <typeparam name="TId">The resource identifier type.</typeparam>
    public interface ICreateProcessor<TResource, TId> : IAtomicOperationProcessor
        where TResource : class, IIdentifiable<TId>
    {
    }
}
