using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Controllers
{
    /// <summary>
    /// The base class to derive resource-specific read-only controllers from.
    /// This class delegates all work to <see cref="BaseJsonApiController{TResource, TId}"/> but adds attributes for routing templates.
    /// If you want to provide routing templates yourself, you should derive from BaseJsonApiController directly.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    /// <typeparam name="TId">The resource identifier type.</typeparam>
    public abstract class JsonApiQueryController<TResource, TId> : BaseJsonApiController<TResource, TId> where TResource : class, IIdentifiable<TId>
    {
        /// <summary>
        /// Creates an instance from a read-only service.
        /// </summary>
        protected JsonApiQueryController(
            IJsonApiOptions context,
            ILoggerFactory loggerFactory,
            IResourceQueryService<TResource, TId> queryService)
            : base(context, loggerFactory, queryService)
        { }

        /// <inheritdoc />
        [HttpGet]
        public override async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
        {
            return await base.GetAsync(cancellationToken);
        }

        /// <inheritdoc />
        [HttpGet("{id}")]
        public override async Task<IActionResult> GetAsync(TId id, CancellationToken cancellationToken)
        {
            return await base.GetAsync(id, cancellationToken);
        }

        /// <inheritdoc />
        [HttpGet("{id}/{relationshipName}")]
        public override async Task<IActionResult> GetSecondaryAsync(TId id, string relationshipName, CancellationToken cancellationToken)
        {
            return await base.GetSecondaryAsync(id, relationshipName, cancellationToken);
        }

        /// <inheritdoc />
        [HttpGet("{id}/relationships/{relationshipName}")]
        public override async Task<IActionResult> GetRelationshipAsync(TId id, string relationshipName, CancellationToken cancellationToken)
        {
            return await base.GetRelationshipAsync(id, relationshipName, cancellationToken);
        }
    }

    /// <inheritdoc />
    public abstract class JsonApiQueryController<TResource> : JsonApiQueryController<TResource, int> where TResource : class, IIdentifiable<int>
    {
        /// <inheritdoc />
        protected JsonApiQueryController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceQueryService<TResource, int> queryService)
            : base(options, loggerFactory, queryService)
        { }
    }
}
