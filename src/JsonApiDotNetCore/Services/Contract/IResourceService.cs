using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IResourceService<T> 
        : IResourceCommandService<T>, IResourceQueryService<T>, IResourceService<T, int> 
        where T : class, IIdentifiable<int>
    { }

    public interface IResourceService<T, in TId> 
        : IResourceCommandService<T, TId>, IResourceQueryService<T, TId>
        where T : class, IIdentifiable<TId>
    { }
}
