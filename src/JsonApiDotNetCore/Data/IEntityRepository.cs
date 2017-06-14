using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Data
{
    public interface IEntityRepository<TEntity>
        : IEntityRepository<TEntity, int>
        where TEntity : class, IIdentifiable<int>
    { }

    public interface IEntityRepository<TEntity, in TId>
        : IEntityReadRepository<TEntity, TId>,  
        IEntityWriteRepository<TEntity, TId>
        where TEntity : class, IIdentifiable<TId>
    { }
}
