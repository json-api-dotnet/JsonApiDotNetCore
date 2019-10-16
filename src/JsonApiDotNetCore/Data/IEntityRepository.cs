using System;
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

    [Obsolete("Do not use anymore. See @MIGRATION_LINK for details.", true)]
    internal interface IEntityFrameworkRepository<TEntity>
    {
        [Obsolete("Do not use anymore. See @MIGRATION_LINK for details.", true)]
        void DetachRelationshipPointers(TEntity entity);
    }
}


