using System.Collections.Generic;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public class UserResource : ResourceDefinition<User>
    {
        protected override List<AttrAttribute> OutputAttrs()
            => Remove(user => user.Password);
            
    }
}
