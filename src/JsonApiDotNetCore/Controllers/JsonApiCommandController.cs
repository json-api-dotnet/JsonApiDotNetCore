using System.Collections.Generic;
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
    /// The base class to derive resource-specific write-only controllers from. This class delegates all work to
    /// <see cref="BaseJsonApiController{TResource, TId}" /> but adds attributes for routing templates. If you want to provide routing templates yourself,
    /// you should derive from BaseJsonApiController directly.
    /// </summary>
    /// <typeparam name="TResource">
    /// The resource type.
    /// </typeparam>
    /// <typeparam name="TId">
    /// The resource identifier type.
    /// </typeparam>
    public abstract class JsonApiCommandController<TResource, TId> : BaseJsonApiController<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        /// <summary>
        /// Creates an instance from a write-only service.
        /// </summary>
        protected JsonApiCommandController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceCommandService<TResource, TId> commandService)
            : base(options, loggerFactory, null, commandService)
        {
        }

        /// <inheritdoc />
        [HttpPost]
        public override async Task<IActionResult> PostAsync([FromBody] TResource resource, CancellationToken cancellationToken)
        {
            return await base.PostAsync(resource, cancellationToken);
        }

        /// <inheritdoc />
        [HttpPost("{id}/relationships/{relationshipName}")]
        public override async Task<IActionResult> PostRelationshipAsync(TId id, string relationshipName, [FromBody] ISet<IIdentifiable> rightResourceIds,
            CancellationToken cancellationToken)
        {
            return await base.PostRelationshipAsync(id, relationshipName, rightResourceIds, cancellationToken);
        }

        /// <inheritdoc />
        [HttpPatch("{id}")]
        public override async Task<IActionResult> PatchAsync(TId id, [FromBody] TResource resource, CancellationToken cancellationToken)
        {
            return await base.PatchAsync(id, resource, cancellationToken);
        }

        /// <inheritdoc />
        [HttpPatch("{id}/relationships/{relationshipName}")]
        public override async Task<IActionResult> PatchRelationshipAsync(TId id, string relationshipName, [FromBody] object? rightValue,
            CancellationToken cancellationToken)
        {
            return await base.PatchRelationshipAsync(id, relationshipName, rightValue, cancellationToken);
        }

        /// <inheritdoc />
        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(TId id, CancellationToken cancellationToken)
        {
            return await base.DeleteAsync(id, cancellationToken);
        }

        /// <inheritdoc />
        [HttpDelete("{id}/relationships/{relationshipName}")]
        public override async Task<IActionResult> DeleteRelationshipAsync(TId id, string relationshipName, [FromBody] ISet<IIdentifiable> rightResourceIds,
            CancellationToken cancellationToken)
        {
            return await base.DeleteRelationshipAsync(id, relationshipName, rightResourceIds, cancellationToken);
        }
    }
}
