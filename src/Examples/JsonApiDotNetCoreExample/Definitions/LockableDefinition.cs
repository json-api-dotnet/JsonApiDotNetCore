using System.Collections.Generic;
using System.Linq;
using System.Net;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using JsonApiDotNetCoreExample.Models;

namespace JsonApiDotNetCoreExample.Definitions
{
    public abstract class LockableDefinition<T> : ResourceDefinition<T> where T : class, IIsLockable, IIdentifiable
    {
        protected LockableDefinition(IResourceGraph resourceGraph) : base(resourceGraph) { }

        protected void DisallowLocked(IEnumerable<T> entities)
        {
            foreach (var e in entities ?? Enumerable.Empty<T>())
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
