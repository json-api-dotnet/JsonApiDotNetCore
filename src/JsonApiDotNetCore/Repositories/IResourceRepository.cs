using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Repositories
{
    /// <summary>
    /// Represents the foundational Resource Repository layer in the JsonApiDotNetCore architecture that provides data access to an underlying store.
    /// </summary>
    /// <typeparam name="TResource">
    /// The resource type.
    /// </typeparam>
    [PublicAPI]
    public interface IResourceRepository<TResource>
        : IResourceRepository<TResource, int>, IResourceReadRepository<TResource>, IResourceWriteRepository<TResource>
        where TResource : class, IIdentifiable<int>
    {
    }

    /// <summary>
    /// Represents the foundational Resource Repository layer in the JsonApiDotNetCore architecture that provides data access to an underlying store.
    /// </summary>
    /// <typeparam name="TResource">
    /// The resource type.
    /// </typeparam>
    /// <typeparam name="TId">
    /// The resource identifier type.
    /// </typeparam>
    public interface IResourceRepository<TResource, in TId> : IResourceReadRepository<TResource, TId>, IResourceWriteRepository<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
    }
}
