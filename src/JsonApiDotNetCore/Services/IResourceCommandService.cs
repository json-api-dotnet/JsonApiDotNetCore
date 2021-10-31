using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Services
{
    /// <summary>
    /// Groups write operations.
    /// </summary>
    /// <typeparam name="TResource">
    /// The resource type.
    /// </typeparam>
    /// <typeparam name="TId">
    /// The resource identifier type.
    /// </typeparam>
    public interface IResourceCommandService<TResource, in TId>
        : ICreateService<TResource, TId>, IAddToRelationshipService<TResource, TId>, IUpdateService<TResource, TId>, ISetRelationshipService<TResource, TId>,
            IDeleteService<TResource, TId>, IRemoveFromRelationshipService<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
    }
}
