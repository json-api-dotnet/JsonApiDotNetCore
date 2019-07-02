using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Hooks
{
    internal interface ITraversalHelper
    {
        EntityChildLayer CreateNextLayer(IEntityNode rootNode);
        EntityChildLayer CreateNextLayer(IEnumerable<IEntityNode> nodes);
        RootNode<TEntity> CreateRootNode<TEntity>(IEnumerable<TEntity> rootEntities) where TEntity : class, IIdentifiable;
    }
}