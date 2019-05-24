using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public class PersonResource : ResourceDefinition<Person>
    {
        public override IEnumerable<string> BeforeUpdateRelationship(HashSet<string> ids, IAffectedRelationships<Person> resourcesByRelationship, ResourcePipeline pipeline)
        {
            BeforeImplicitUpdateRelationship(resourcesByRelationship, pipeline);
            return ids;
        }

        public override void BeforeImplicitUpdateRelationship(IAffectedRelationships<Person> resourcesByRelationship, ResourcePipeline pipeline)
        {
            resourcesByRelationship.GetByRelationship<Passport>().ToList().ForEach(kvp => DoesNotTouchLockedPeople(kvp.Value));
        }

        private void DoesNotTouchLockedPeople(IEnumerable<Person> entities)
        {
            foreach (var person in entities ?? Enumerable.Empty<Person>())
            {
                if (person.IsLocked)
                {
                    throw new JsonApiException(403, "Not allowed to update fields or relations of locked persons", new UnauthorizedAccessException());
                }
            }
        }
    }
}
