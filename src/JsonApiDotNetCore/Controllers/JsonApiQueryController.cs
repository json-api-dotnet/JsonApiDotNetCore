using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Controllers
{
    /// <summary>
    /// The base class to derive resource-specific read-only controllers from. Returns HTTP 405 on write-only endpoints. If you want to provide routing
    /// templates yourself, you should derive from BaseJsonApiController directly.
    /// </summary>
    /// <typeparam name="TResource">
    /// The resource type.
    /// </typeparam>
    /// <typeparam name="TId">
    /// The resource identifier type.
    /// </typeparam>
    public abstract class JsonApiQueryController<TResource, TId> : JsonApiController<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        /// <summary>
        /// Creates an instance from a read-only service.
        /// </summary>
        protected JsonApiQueryController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IResourceQueryService<TResource, TId> queryService)
            : base(options, resourceGraph, loggerFactory, queryService, queryService, queryService, queryService)
        {
        }
    }
}
