using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public class PersonResource : ResourceDefinition<Person>
    {
        public override IEnumerable<Person> BeforeUpdate(IEnumerable<Person> entities, ResourceAction actionSource) { return entities; }
        public override IEnumerable<Person> AfterUpdate(IEnumerable<Person> entities,  ResourceAction actionSource) { return entities; }
    }
}
