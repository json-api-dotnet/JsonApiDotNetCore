using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public class PassportResource : ResourceDefinition<Passport>
    {
        public override void BeforeRead(ResourceAction pipeline, bool nestedHook = false, string stringId = null)
        {
            if (pipeline == ResourceAction.GetSingle && nestedHook)
            {
                throw new JsonApiException(403, "Not allowed to include passports on individual people", new UnauthorizedAccessException());

            }
        }
    }
}
