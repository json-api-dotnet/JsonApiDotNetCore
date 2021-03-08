using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

// ReSharper disable UnusedTypeParameter

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <summary>
    /// Processes a single operation to update the attributes and/or relationships of an existing resource. Only the values of sent attributes are replaced.
    /// And only the values of sent relationships are replaced.
    /// </summary>
    /// <typeparam name="TResource">
    /// The resource type.
    /// </typeparam>
    /// <typeparam name="TId">
    /// The resource identifier type.
    /// </typeparam>
    [PublicAPI]
    public interface IUpdateProcessor<TResource, TId> : IOperationProcessor
        where TResource : class, IIdentifiable<TId>
    {
    }
}
