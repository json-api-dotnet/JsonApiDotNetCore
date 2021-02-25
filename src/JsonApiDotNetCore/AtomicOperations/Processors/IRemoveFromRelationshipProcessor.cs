using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <summary>
    /// Processes a single operation to remove resources from a to-many relationship.
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    /// <typeparam name="TId"></typeparam>
    [PublicAPI]
    public interface IRemoveFromRelationshipProcessor<TResource, TId> : IOperationProcessor
        where TResource : class, IIdentifiable<TId>
    {
    }
}
