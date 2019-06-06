using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public class PersonResource : LockableResourceBase<Person>
    {
        public PersonResource(IResourceGraph graph) : base(graph) { }

        public override IEnumerable<string> BeforeUpdateRelationship(HashSet<string> ids, IAffectedRelationships<Person> resourcesByRelationship, ResourcePipeline pipeline)
        {
            BeforeImplicitUpdateRelationship(resourcesByRelationship, pipeline);
            return ids;
        }

        //[LoadDatabaseValues(true)]
        //public override IEnumerable<Person> BeforeUpdate(IResourceDiff<Person> entityDiff, ResourcePipeline pipeline)
        //{
        //    return entityDiff.Entities;
        //}

        public override void BeforeImplicitUpdateRelationship(IAffectedRelationships<Person> resourcesByRelationship, ResourcePipeline pipeline)
        {
            resourcesByRelationship.GetByRelationship<Passport>().ToList().ForEach(kvp => DisallowLocked(kvp.Value));
        }
    }
}
