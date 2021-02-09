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
        private readonly IResourceGraph _resourceGraph;
        protected LockableHooksDefinition(IResourceGraph resourceGraph) : base(resourceGraph)
        {
            _resourceGraph = resourceGraph;
        }

        protected void DisallowLocked(IEnumerable<T> resources)
        {
            foreach (var resource in resources ?? Enumerable.Empty<T>())
            {
                if (resource.IsLocked)
                {
                    throw new JsonApiException(new Error(HttpStatusCode.Forbidden)
                    {
                        Title = $"You are not allowed to update fields or relationships of locked resource of type '{_resourceGraph.GetResourceContext<T>().PublicName}'."
                    });
                }
            }
        }
    }
}
