using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IResourceQueryService<T> : 
        IGetAllService<T>,
        IGetByIdService<T>,
        IGetRelationshipsService<T>,
        IGetRelationshipService<T>,
        IResourceQueryService<T, int>
        where T : class, IIdentifiable<int>
    { }

    public interface IResourceQueryService<T, in TId> :
        IGetAllService<T, TId>,
        IGetByIdService<T, TId>,
        IGetRelationshipsService<T, TId>,
        IGetRelationshipService<T, TId>
        where T : class, IIdentifiable<TId>
    { }
}
