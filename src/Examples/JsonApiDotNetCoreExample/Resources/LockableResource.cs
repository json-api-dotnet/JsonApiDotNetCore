using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Resources
{
    public abstract class LockableResource<T> : ResourceDefinition<T> where T : class, IIsLockable, IIdentifiable
    {
        protected LockableResource(IContextEntityProvider provider) : base(provider) { }

        protected void DisallowLocked(IEnumerable<T> entities)
        {
            foreach (var e in entities ?? Enumerable.Empty<T>())
            {
                if (e.IsLocked)
                {
                    throw new JsonApiException(403, "Not allowed to update fields or relations of locked todo item", new UnauthorizedAccessException());
                }
            }
        }
    }
}
