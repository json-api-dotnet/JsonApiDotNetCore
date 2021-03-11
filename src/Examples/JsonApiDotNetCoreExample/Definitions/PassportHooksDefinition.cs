using System.Collections.Generic;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Definitions
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class PassportHooksDefinition : LockableHooksDefinition<Passport>
    {
        public PassportHooksDefinition(IResourceGraph resourceGraph)
            : base(resourceGraph)
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
            resourcesByRelationship.GetByRelationship<Person>().ToList().ForEach(pair => DisallowLocked(pair.Value));
        }

        public override IEnumerable<Passport> OnReturn(HashSet<Passport> resources, ResourcePipeline pipeline)
        {
            return resources.Where(passport => !passport.IsLocked).ToArray();
        }
    }
}
