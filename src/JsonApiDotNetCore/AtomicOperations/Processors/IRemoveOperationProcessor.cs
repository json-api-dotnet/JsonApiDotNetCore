using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <inheritdoc />
    public interface IRemoveOperationProcessor<TResource> : IRemoveOperationProcessor<TResource, int>
        where TResource : class, IIdentifiable<int>
    {
    }

    /// <summary>
    /// Processes a single operation with code <see cref="AtomicOperationCode.Remove"/> in a list of atomic operations.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    /// <typeparam name="TId">The resource identifier type.</typeparam>
    public interface IRemoveOperationProcessor<TResource, TId> : IAtomicOperationProcessor
        where TResource : class, IIdentifiable<TId>
    {
    }
}
