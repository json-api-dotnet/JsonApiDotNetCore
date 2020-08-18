using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IResourceQueryService<TResource> : 
        IGetAllService<TResource>,
        IGetByIdService<TResource>,
        IGetRelationshipService<TResource>,
        IGetSecondaryService<TResource>,
        IResourceQueryService<TResource, int>
        where TResource : class, IIdentifiable<int>
    { }

    public interface IResourceQueryService<TResource, in TId> :
        IGetAllService<TResource, TId>,
        IGetByIdService<TResource, TId>,
        IGetRelationshipService<TResource, TId>,
        IGetSecondaryService<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    { }
}
