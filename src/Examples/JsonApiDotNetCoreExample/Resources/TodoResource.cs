using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public class TodoResource : ResourceDefinition<TodoItem>
    {

        public override IEnumerable<TodoItem> BeforeUpdate(EntityDiff<TodoItem> entityDiff, HookExecutionContext<TodoItem> context)
        {
            var entitiesInBody = entityDiff.RequestEntities;
            foreach (var todo in entitiesInBody)
            {
                if (todo.IsLocked)
                {
                    throw new JsonApiException(401, "Not allowed to update fields or relations of locked todo item", new UnauthorizedAccessException());
                }
            }

            if (context.GetEntitiesForAffectedRelationship<Person>().Any( pair => pair.Key.InternalRelationshipName == "Author" ))
            {
                throw new JsonApiException(401, "Not allowed to update author for any TodoItem", new UnauthorizedAccessException());
            }


        }

        public override IEnumerable<TodoItem> BeforeUpdate(IEnumerable<TodoItem> entities, ResourceAction actionSource)
        {
            foreach (var todo in entities)
            {
                if (todo.IsLocked)
                {
                    throw new JsonApiException(401, "Not allowed fields or relations of locked todo item", new UnauthorizedAccessException());
                }
            }

            return entities;
        }
    }

}
