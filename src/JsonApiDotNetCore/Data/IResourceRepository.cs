using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Data
{
    public interface IResourceRepository<TEntity>
        : IResourceRepository<TEntity, int>
        where TEntity : class, IIdentifiable<int>
    { }

    public interface IResourceRepository<TEntity, in TId>
        : IResourceReadRepository<TEntity, TId>,
        IResourceWriteRepository<TEntity, TId>
        where TEntity : class, IIdentifiable<TId>
    { }
}