using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public class PassportResource : ResourceDefinition<Passport>
    {
        public override void BeforeRead(ResourcePipeline pipeline, bool nestedHook = false, string stringId = null)
        {
            if (pipeline == ResourcePipeline.ReadSingle && nestedHook)
            {
                throw new JsonApiException(403, "Not allowed to include passports on individual people", new UnauthorizedAccessException());
            }
        }

        public override void BeforeImplicitUpdateRelationship(IAffectedRelationships<Passport> resourcesByRelationship, ResourcePipeline pipeline)
        {
            resourcesByRelationship.GetByRelationship<Person>().ToList().ForEach(kvp => DoesNotTouchLockedPassports(kvp.Value));
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
