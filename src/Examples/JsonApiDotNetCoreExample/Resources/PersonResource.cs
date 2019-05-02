using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Hooks.Discovery;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public class PersonResource : ResourceDefinition<Person>
    {

        [DatabaseValuesInDiffs(false)]
        public override IEnumerable<Person> BeforeUpdate(EntityDiff<Person> entityDiff, HookExecutionContext<Person> context)
        {
            var entitiesInBody = entityDiff.RequestEntities;
            foreach (var person in entitiesInBody)
            {
                if (person.IsLocked)
                {
                    throw new JsonApiException(401, "Not allowed to update fields or relations of locked todo item", new UnauthorizedAccessException());
                }
            }
            return entityDiff.RequestEntities;
        }
    }
}
