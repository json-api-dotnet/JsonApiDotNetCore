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

        public override void BeforeImplicitUpdateRelationship(IUpdatedRelationshipHelper<TodoItem> relationshipHelper, ResourceAction pipeline)
        {
            List<TodoItem> todos = relationshipHelper.EntitiesRelatedTo<Person>().SelectMany(kvp => kvp.Value).ToList();
            DoesNotTouchLocked(todos);
        }


        private void DoesNotTouchLocked(IEnumerable<TodoItem> entities)
        {
            foreach (var person in entities ?? Enumerable.Empty<TodoItem>())
            {
                if (person.IsLocked)
                {
                    throw new JsonApiException(403, "Not allowed to update fields or relations of locked todo item", new UnauthorizedAccessException());
                }
            }
        }
    }
}
