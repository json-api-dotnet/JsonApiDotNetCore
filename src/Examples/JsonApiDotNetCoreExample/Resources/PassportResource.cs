using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models.JsonApiDocuments;

namespace JsonApiDotNetCoreExample.Resources
{
    public class PassportResource : ResourceDefinition<Passport>
    {
        public PassportResource(IResourceGraph resourceGraph) : base(resourceGraph)
        {
        }

        public override void BeforeRead(ResourcePipeline pipeline, bool isIncluded = false, string stringId = null)
        {
            if (pipeline == ResourcePipeline.GetSingle && isIncluded)
            {
                throw new JsonApiException(new Error(HttpStatusCode.Forbidden)
                {
                    Title = "You are not allowed to include passports on individual persons."
                });
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
                    throw new JsonApiException(new Error(HttpStatusCode.Forbidden)
                    {
                        Title = "You are not allowed to update fields or relations of locked persons."
                    });
                }
            }
        }
    }
}
