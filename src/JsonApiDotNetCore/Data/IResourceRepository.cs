using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Data
{
    public interface IResourceRepository<TResource>
        : IResourceRepository<TResource, int>
        where TResource : class, IIdentifiable<int>
    { }

    public interface IResourceRepository<TResource, in TId>
        : IResourceReadRepository<TResource, TId>,
        IResourceWriteRepository<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    { }
}