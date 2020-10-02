using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Controllers
{
    /// <summary>
    /// The base class to derive resource-specific controllers from.
    /// This class delegates all work to <see cref="BaseJsonApiController{TResource, TId}"/> but adds attributes for routing templates.
    /// If you want to provide routing templates yourself, you should derive from BaseJsonApiController directly.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    /// <typeparam name="TId">The resource identifier type.</typeparam>
    public class JsonApiController<TResource, TId> : BaseJsonApiController<TResource, TId> where TResource : class, IIdentifiable<TId>
    {
        /// <inheritdoc />
        public JsonApiController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<TResource, TId> resourceService)
            : base(options, loggerFactory, resourceService)
        { }

        /// <inheritdoc />
        public JsonApiController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IGetAllService<TResource, TId> getAll = null,
            IGetByIdService<TResource, TId> getById = null,
            IGetSecondaryService<TResource, TId> getSecondary = null,
            IGetRelationshipService<TResource, TId> getRelationship = null,
            ICreateService<TResource, TId> create = null,
            IUpdateService<TResource, TId> update = null,
            IUpdateRelationshipService<TResource, TId> updateRelationships = null,
            IDeleteService<TResource, TId> delete = null)
            : base(options, loggerFactory, getAll, getById, getSecondary, getRelationship, create, update,
                updateRelationships, delete)
        { }

        /// <inheritdoc />
        [HttpGet]
        public override async Task<IActionResult> GetAsync() => await base.GetAsync();

        /// <inheritdoc />
        [HttpGet("{id}")]
        public override async Task<IActionResult> GetAsync(TId id) => await base.GetAsync(id);

        /// <inheritdoc />
        [HttpGet("{id}/relationships/{relationshipName}")]
        public override async Task<IActionResult> GetRelationshipAsync(TId id, string relationshipName)
            => await base.GetRelationshipAsync(id, relationshipName);

        /// <inheritdoc />
        [HttpGet("{id}/{relationshipName}")]
        public override async Task<IActionResult> GetSecondaryAsync(TId id, string relationshipName)
            => await base.GetSecondaryAsync(id, relationshipName);

        /// <inheritdoc />
        [HttpPost]
        public override async Task<IActionResult> PostAsync([FromBody] TResource resource)
            => await base.PostAsync(resource);

        /// <inheritdoc />
        [HttpPatch("{id}")]
        public override async Task<IActionResult> PatchAsync(TId id, [FromBody] TResource resource)
        {
            return await base.PatchAsync(id, resource);
        }

        /// <inheritdoc />
        [HttpPatch("{id}/relationships/{relationshipName}")]
        public override async Task<IActionResult> PatchRelationshipAsync(
            TId id, string relationshipName, [FromBody] object relationships)
            => await base.PatchRelationshipAsync(id, relationshipName, relationships);

        /// <inheritdoc />
        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(TId id) => await base.DeleteAsync(id);
    }

    /// <inheritdoc />
    public class JsonApiController<TResource> : JsonApiController<TResource, int> where TResource : class, IIdentifiable<int>
    {
        /// <inheritdoc />
        public JsonApiController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<TResource, int> resourceService)
            : base(options, loggerFactory, resourceService)
        { }

        /// <inheritdoc />
        public JsonApiController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IGetAllService<TResource, int> getAll = null,
            IGetByIdService<TResource, int> getById = null,
            IGetSecondaryService<TResource, int> getSecondary = null,
            IGetRelationshipService<TResource, int> getRelationship = null,
            ICreateService<TResource, int> create = null,
            IUpdateService<TResource, int> update = null,
            IUpdateRelationshipService<TResource, int> updateRelationships = null,
            IDeleteService<TResource, int> delete = null)
            : base(options, loggerFactory, getAll, getById, getSecondary, getRelationship, create, update,
                updateRelationships, delete)
        { }
    }
}
