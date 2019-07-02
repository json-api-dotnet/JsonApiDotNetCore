using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCore.Internal.Contracts;

namespace JsonApiDotNetCoreExample.Resources
{
    public class PassportResource : ResourceDefinition<Passport>
    {
        public PassportResource(IResourceGraph graph) : base(graph)
        {
        }

        public override void BeforeRead(ResourcePipeline pipeline, bool isIncluded = false, string stringId = null)
        {
            if (pipeline == ResourcePipeline.GetSingle && isIncluded)
            {
                throw new JsonApiException(403, "Not allowed to include passports on individual people", new UnauthorizedAccessException());
            }
        }

        public override void BeforeImplicitUpdateRelationship(IRelationshipsDictionary<Passport> resourcesByRelationship, ResourcePipeline pipeline)
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
