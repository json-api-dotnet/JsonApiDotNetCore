using System.Collections.Generic;
using System.Linq;
using System.Net;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Definitions
{
    public abstract class LockableHooksDefinition<T> : ResourceHooksDefinition<T> where T : class, IIsLockable, IIdentifiable
    {
        protected LockableHooksDefinition(IResourceGraph resourceGraph) : base(resourceGraph) { }

        protected void DisallowLocked(IEnumerable<T> resources)
        {
            foreach (var e in resources ?? Enumerable.Empty<T>())
            {
                if (e.IsLocked)
                {
                    throw new JsonApiException(new Error(HttpStatusCode.Forbidden)
                    {
                        Title = "You are not allowed to update fields or relationships of locked todo items."
                    });
                }
            }
        }
    }
}
