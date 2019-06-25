using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public class PersonResource : LockableResource<Person>
    {
        public PersonResource(IResourceGraph graph) : base(graph) { }

        public override IEnumerable<string> BeforeUpdateRelationship(HashSet<string> ids, IRelationshipsDictionary<Person> entitiesByRelationship, ResourcePipeline pipeline)
        {
            BeforeImplicitUpdateRelationship(entitiesByRelationship, pipeline);
            return ids;
        }

        public override void BeforeImplicitUpdateRelationship(IRelationshipsDictionary<Person> entitiesByRelationship, ResourcePipeline pipeline)
        {
            entitiesByRelationship.GetByRelationship<Passport>().ToList().ForEach(kvp => DisallowLocked(kvp.Value));
        }
    }
}
