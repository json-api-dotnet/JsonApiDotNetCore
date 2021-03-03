using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Services
{
    /// <summary>
    /// Groups read operations.
    /// </summary>
    /// <typeparam name="TResource">
    /// The resource type.
    /// </typeparam>
    public interface IResourceQueryService<TResource>
        : IGetAllService<TResource>, IGetByIdService<TResource>, IGetRelationshipService<TResource>, IGetSecondaryService<TResource>,
            IResourceQueryService<TResource, int>
        where TResource : class, IIdentifiable<int>
    {
    }

    /// <summary>
    /// Groups read operations.
    /// </summary>
    /// <typeparam name="TResource">
    /// The resource type.
    /// </typeparam>
    /// <typeparam name="TId">
    /// The resource identifier type.
    /// </typeparam>
    public interface IResourceQueryService<TResource, in TId>
        : IGetAllService<TResource, TId>, IGetByIdService<TResource, TId>, IGetRelationshipService<TResource, TId>, IGetSecondaryService<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
    }
}
