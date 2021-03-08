using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

// ReSharper disable UnusedTypeParameter

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <summary>
    /// Processes a single operation to create a new resource with attributes, relationships or both.
    /// </summary>
    /// <typeparam name="TResource">
    /// The resource type.
    /// </typeparam>
    /// <typeparam name="TId">
    /// The resource identifier type.
    /// </typeparam>
    [PublicAPI]
    public interface ICreateProcessor<TResource, TId> : IOperationProcessor
        where TResource : class, IIdentifiable<TId>
    {
    }
}
