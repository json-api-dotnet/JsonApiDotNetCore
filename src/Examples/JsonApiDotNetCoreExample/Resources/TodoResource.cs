using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    //public class HookDiff<TEntity>
    //{
    //    public IEnumerable<TEntity> RequestEntities;
    //    public IEnumerable<TEntity> DatabaseEntities;
         
    //}
    //public interface IHookContext { }
    //public class HookExecutionContext<TEntity> : IHookContext
    //{
    //    ResourceAction ServiceAction { get; set; }
    //    private Dictionary<string, List<TEntity>> UpdatedRelations { get; set; }
    //    //public IEnumerable<TEntity> DatabaseEntities;

    //    //public List<IRelationship> GetUpdatedRelationships<TEntity>(TEntity entity);

    //}


    public class TodoResource : ResourceDefinition<TodoItem>
    {

        public override IEnumerable<TodoItem> BeforeUpdate(IEnumerable<TodoItem> entities, ResourceAction actionSource)
        {
            foreach (var todo in entities)
            {
                if (todo.IsLocked)
                {
                    throw new JsonApiException(401, "Not allowed update fields on locked todo item", new UnauthorizedAccessException());
                }
            }

            return entities;
        }
    }

}
