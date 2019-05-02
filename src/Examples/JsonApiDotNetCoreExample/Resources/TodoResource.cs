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

            var relationshipsToPerson = context.GetEntitiesForAffectedRelationship<Person>();
            if (relationshipsToPerson != null && relationshipsToPerson.Any(pair => pair.Key.InternalRelationshipName == "Author"))
            {
                throw new JsonApiException(401, "Not allowed to update author of any TodoItem", new UnauthorizedAccessException());
            }

            // ignore any updates for items with ordinal bigger than 1000 (for whatever reason).
            return entitiesInBody.Where(td => td.Ordinal < 1000);
        }
    }
}
