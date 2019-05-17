using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public class PassportResource : ResourceDefinition<Passport>
    {
        public override void BeforeRead(ResourceAction pipeline, bool nestedHook = false, string stringId = null)
        {
            if (pipeline == ResourceAction.GetSingle && nestedHook)
            {
                throw new JsonApiException(403, "Not allowed to include passports on individual people", new UnauthorizedAccessException());
            }
        }

        public override void BeforeImplicitUpdateRelationship(IUpdatedRelationshipHelper<Passport> relationshipHelper, ResourceAction pipeline)
        {
            relationshipHelper.EntitiesRelatedTo<Person>().ToList().ForEach(kvp => DoesNotTouchLockedPassports(kvp.Value));
        }

        private void DoesNotTouchLockedPassports(IEnumerable<Passport> entities)
        {
            foreach (var entity in entities ?? Enumerable.Empty<Passport>())
            {
                if (entity.IsLocked)
                {
                    throw new JsonApiException(403, "Not allowed to update fields or relations of locked persons", new UnauthorizedAccessException());
                }
            }
        }
    }
}
