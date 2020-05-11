using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Definitions
{
    public class PersonDefinition : LockableDefinition<Person>, IHasMeta
    {
        public PersonDefinition(IResourceGraph resourceGraph) : base(resourceGraph) { }

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
                { "authors", new[] { "Jared Nance", "Maurits Moeys", "Harro van der Kroft" } }
            };
        }
    }
}
