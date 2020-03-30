using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public abstract class LockableResource<T> : ResourceDefinition<T> where T : class, IIsLockable, IIdentifiable
    {
        protected LockableResource(IResourceGraph resourceGraph) : base(resourceGraph) { }

        protected void DisallowLocked(IEnumerable<T> entities)
        {
            foreach (var e in entities ?? Enumerable.Empty<T>())
            {
                if (e.IsLocked)
                {
                    throw new JsonApiException(HttpStatusCode.Forbidden, "Not allowed to update fields or relations of locked todo item", new UnauthorizedAccessException());
                }
            }
        }
    }
}
