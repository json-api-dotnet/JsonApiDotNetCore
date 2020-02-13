using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IResourceCommandService<T> : 
        ICreateService<T>,
        IUpdateService<T>,
        IUpdateRelationshipService<T>,
        IDeleteService<T>,
        IResourceCommandService<T, int>
        where T : class, IIdentifiable<int>
    { }

    public interface IResourceCommandService<T, in TId> : 
        ICreateService<T, TId>,
        IUpdateService<T, TId>,
        IUpdateRelationshipService<T, TId>,
        IDeleteService<T, TId>
        where T : class, IIdentifiable<TId>
    { }
}
