using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IEntityDiff<TEntity> where TEntity : class, IIdentifiable
    {
        IEnumerable<TEntity> RequestEntities { get; }
        IEnumerable<TEntity> DatabaseEntities { get; }
    }

    public class EntityDiff<TEntity> : IEntityDiff<TEntity> where TEntity : class, IIdentifiable
    {
        public IEnumerable<TEntity> RequestEntities { get; private set; }
        public IEnumerable<TEntity> DatabaseEntities { get; private set; }
        public EntityDiff(IEnumerable<TEntity> requestEntities, IEnumerable<TEntity> databaseEntities)
        {
            RequestEntities = requestEntities;
            DatabaseEntities = databaseEntities;
        }
    }
}
