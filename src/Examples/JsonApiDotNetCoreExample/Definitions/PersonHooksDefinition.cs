using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Hooks.Internal.Execution;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Definitions
{
    public class PersonHooksDefinition : LockableHooksDefinition<Person>
    {
        public PersonHooksDefinition(IResourceGraph resourceGraph) : base(resourceGraph) { }

        public override IEnumerable<string> BeforeUpdateRelationship(HashSet<string> ids, IRelationshipsDictionary<Person> resourcesByRelationship, ResourcePipeline pipeline)
        {
            BeforeImplicitUpdateRelationship(resourcesByRelationship, pipeline);
            return ids;
        }

        public override void BeforeImplicitUpdateRelationship(IRelationshipsDictionary<Person> resourcesByRelationship, ResourcePipeline pipeline)
        {
            resourcesByRelationship.GetByRelationship<Passport>().ToList().ForEach(kvp => DisallowLocked(kvp.Value));
        }
    }
}
