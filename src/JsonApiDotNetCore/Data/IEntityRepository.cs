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

    /// <summary>
    /// A staging interface to avoid breaking changes that 
    /// specifically depend on EntityFramework.
    /// </summary>
    internal interface IEntityFrameworkRepository<TEntity>
    {
        /// <summary>
        /// Ensures that any relationship pointers created during a POST or PATCH
        /// request are detached from the DbContext.
        /// This allows the relationships to be fully loaded from the database.
        /// 
        /// </summary>
        /// <remarks>
        /// The only known case when this should be called is when a POST request is
        /// sent with an ?include query.
        /// 
        /// See https://github.com/json-api-dotnet/JsonApiDotNetCore/issues/343
        /// </remarks>
        void DetachRelationshipPointers(TEntity entity);
    }

}


