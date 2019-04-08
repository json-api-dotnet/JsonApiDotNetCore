using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public class UserResource : ResourceDefinition<User>
    {
        protected override List<AttrAttribute> OutputAttrs()
            => Remove(user => user.Password);

        public override IEnumerable<User> AfterRead(IEnumerable<User> entities, ResourceAction actionSource)
        {
            return entities;
        }
    }
}
