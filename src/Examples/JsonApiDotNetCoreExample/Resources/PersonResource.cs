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
    public class PersonResource : ResourceDefinition<Person>
    {
        public override IEnumerable<string> BeforeUpdateRelationship(IEnumerable<string> ids, IUpdatedRelationshipHelper<Person> relationshipHelper, ResourceAction pipeline)
        {
            BeforeImplicitUpdateRelationship(relationshipHelper, pipeline);
            return ids;
        }

        public override void BeforeImplicitUpdateRelationship(IUpdatedRelationshipHelper<Person> relationshipHelper, ResourceAction pipeline)
        {
            relationshipHelper.GetEntitiesRelatedWith<Passport>()
                        .ToList().ForEach(kvp => DoesNotTouchLockedPeople(kvp.Value));

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
