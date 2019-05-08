using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Hooks.Discovery;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public class TodoResource : ResourceDefinition<TodoItem>
    {
        public override void BeforeRead(ResourceAction pipeline, bool nestedHook = false, string stringId = null)
        {
            if (stringId == "1337")
            {
                throw new JsonApiException(403, "Not allowed to update author of any TodoItem", new UnauthorizedAccessException());
            }
        }


        //[DatabaseValuesInDiffs(false)]
        //public override IEnumerable<TodoItem> BeforeUpdate(EntityDiff<TodoItem> entityDiff, HookExecutionContext<TodoItem> context)
        //{
        //    var dbEntities = entityDiff.DatabaseEntities;
        //    DoesNotTouchLocked(dbEntities);

        //    var relationshipsToPerson = context.GetEntitiesRelatedWith<Person>();
        //    if (relationshipsToPerson != null && relationshipsToPerson.Any(pair => pair.Key.InternalRelationshipName == "Author"))
        //    {
        //        throw new JsonApiException(401, "Not allowed to update author of any TodoItem", new UnauthorizedAccessException());
        //    }

        //    // ignore any updates for items with ordinal bigger than 1000 (for whatever reason).
        //    return entityDiff.RequestEntities.Where(td => td.Ordinal <= 1000);
        //}


        //public override void ImplicitUpdateRelationship(IEnumerable<TodoItem> entities, RelationshipAttribute affectedRelationship)
        //{
        //    DoesNotTouchLocked(entities);
        //}


        //public override IEnumerable<TodoItem> BeforeDelete(IEnumerable<TodoItem> entities, HookExecutionContext<TodoItem> context)
        //{
        //    return base.BeforeDelete(entities, context);
        //}

        //private void DoesNotTouchLocked(IEnumerable<TodoItem> entities)
        //{
        //    foreach (var person in entities ?? Enumerable.Empty<TodoItem>())
        //    {
        //        if (person.IsLocked)
        //        {
        //            throw new JsonApiException(403, "Not allowed to update fields or relations of locked todo item", new UnauthorizedAccessException());
        //        }
        //    }
        //}
    }
}
