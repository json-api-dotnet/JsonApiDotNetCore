using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCoreExample.Models;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public class PersonResource : LockableResource<Person>, IHasMeta
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


        public Dictionary<string, object> GetMeta()
        {
            return new Dictionary<string, object> {
                { "copyright", "Copyright 2015 Example Corp." },
                { "authors", new string[] { "Jared Nance" } }
            };
        }
    }
}
