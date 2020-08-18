using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IResourceService<TResource> 
        : IResourceCommandService<TResource>, IResourceQueryService<TResource>, IResourceService<TResource, int> 
        where TResource : class, IIdentifiable<int>
    { }

    public interface IResourceService<TResource, in TId> 
        : IResourceCommandService<TResource, TId>, IResourceQueryService<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    { }
}
