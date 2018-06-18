using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IResourceService<T> 
        : IResourceCmdService<T>, IResourceQueryService<T>, IResourceService<T, int> 
        where T : class, IIdentifiable<int>
    { }

    public interface IResourceService<T, in TId> 
        : IResourceCmdService<T, TId>, IResourceQueryService<T, TId>
        where T : class, IIdentifiable<TId>
    { }
}
