using System.Collections.Generic;

namespace JsonApiDotNetCore.Services
{
    public interface IResourceLogicExecutor<TEntity>
    {
        IList<TEntity> ApplyLogic(IList<TEntity> entities, string rel);
    }
}