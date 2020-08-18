using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Services
{
    public interface IResourceCommandService<TResource> : 
        ICreateService<TResource>,
        IUpdateService<TResource>,
        IUpdateRelationshipService<TResource>,
        IDeleteService<TResource>,
        IResourceCommandService<TResource, int>
        where TResource : class, IIdentifiable<int>
    { }

    public interface IResourceCommandService<TResource, in TId> : 
        ICreateService<TResource, TId>,
        IUpdateService<TResource, TId>,
        IUpdateRelationshipService<TResource, TId>,
        IDeleteService<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    { }
}
