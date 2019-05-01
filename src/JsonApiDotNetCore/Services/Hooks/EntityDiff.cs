using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IEntityDiff<TEntity> where TEntity : class, IIdentifiable
    {
        HashSet<TEntity> RequestEntities { get; }
        IEnumerable<TEntity> DatabaseEntities { get; }
    }

    public class EntityDiff<TEntity> : IEntityDiff<TEntity> where TEntity : class, IIdentifiable
    {
        public HashSet<TEntity> RequestEntities { get; private set; }
        public IEnumerable<TEntity> DatabaseEntities { get; private set; }
        public EntityDiff(HashSet<TEntity> requestEntities, IEnumerable<TEntity> databaseEntities)
        {
            RequestEntities = requestEntities;
            DatabaseEntities = databaseEntities;
        }
    }
}
