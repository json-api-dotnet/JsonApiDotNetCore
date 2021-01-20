using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <inheritdoc />
    public interface IDeleteProcessor<TResource> : IDeleteProcessor<TResource, int>
        where TResource : class, IIdentifiable<int>
    {
    }

    /// <summary>
    /// Processes a single operation to delete an existing resource.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    /// <typeparam name="TId">The resource identifier type.</typeparam>
    public interface IDeleteProcessor<TResource, TId> : IOperationProcessor
        where TResource : class, IIdentifiable<TId>
    {
    }
}
