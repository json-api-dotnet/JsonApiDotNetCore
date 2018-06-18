using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IResourceCmdService<T> : 
        ICreateService<T>,
        IUpdateService<T>,
        IUpdateRelationshipService<T>,
        IDeleteService<T>,
        IResourceCmdService<T, int>
        where T : class, IIdentifiable<int>
    { }

    public interface IResourceCmdService<T, in TId> : 
        ICreateService<T, TId>,
        IUpdateService<T, TId>,
        IUpdateRelationshipService<T, TId>,
        IDeleteService<T, TId>
        where T : class, IIdentifiable<TId>
    { }
}
