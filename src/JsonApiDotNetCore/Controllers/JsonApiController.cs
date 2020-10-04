using System.Collections.Generic;
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
            ICreateService<TResource, TId> create = null,
            IGetAllService<TResource, TId> getAll = null,
            IGetByIdService<TResource, TId> getById = null,
            IGetSecondaryService<TResource, TId> getSecondary = null,
            IUpdateService<TResource, TId> update = null,
            IDeleteService<TResource, TId> delete = null,
            IAddRelationshipService<TResource, TId> addRelationship = null,
            IGetRelationshipService<TResource, TId> getRelationship = null,
            ISetRelationshipService<TResource, TId> setRelationship = null,
            IDeleteRelationshipService<TResource, TId> deleteRelationship = null)
            : base(options, loggerFactory, create, getAll, getById, getSecondary, update, delete, addRelationship, 
                getRelationship, setRelationship, deleteRelationship)
        { }

        #region Primary Resource Endpoints

        /// <inheritdoc />
        [HttpPost]
        public override async Task<IActionResult> PostAsync([FromBody] TResource resource)
            => await base.PostAsync(resource);
        
        /// <inheritdoc />
        [HttpGet]
        public override async Task<IActionResult> GetAsync() => await base.GetAsync();

        /// <inheritdoc />
        [HttpGet("{id}")]
        public override async Task<IActionResult> GetAsync(TId id) => await base.GetAsync(id);
        
        /// <inheritdoc />
        [HttpGet("{id}/{relationshipName}")]
        public override async Task<IActionResult> GetSecondaryAsync(TId id, string relationshipName)
            => await base.GetSecondaryAsync(id, relationshipName);
        

        /// <inheritdoc />
        [HttpPatch("{id}")]
        public override async Task<IActionResult> PatchAsync(TId id, [FromBody] TResource resource)
        {
            return await base.PatchAsync(id, resource);
        }

        /// <inheritdoc />
        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(TId id) => await base.DeleteAsync(id);
        
        #endregion
        
        #region Relationship Link Endpoints

        /// <inheritdoc />
        [HttpPost("{id}/relationships/{relationshipName}")]
        public override async Task<IActionResult> PostRelationshipAsync(
            TId id, string relationshipName, [FromBody] IEnumerable<IIdentifiable> relationships)
            => await base.PostRelationshipAsync(id, relationshipName, relationships);
        
        /// <inheritdoc />
        [HttpGet("{id}/relationships/{relationshipName}")]
        public override async Task<IActionResult> GetRelationshipAsync(TId id, string relationshipName)
            => await base.GetRelationshipAsync(id, relationshipName);
        
        /// <inheritdoc />
        [HttpPatch("{id}/relationships/{relationshipName}")]
        public override async Task<IActionResult> PatchRelationshipAsync(TId id, string relationshipName, [FromBody] object relationships)
            => await base.PatchRelationshipAsync(id, relationshipName, relationships);
        
        /// <inheritdoc />
        [HttpDelete("{id}/relationships/{relationshipName}")]
        public override async Task<IActionResult> DeleteRelationshipAsync(TId id, string relationshipName, [FromBody] IEnumerable<IIdentifiable> relationships)
            => await base.DeleteRelationshipAsync(id, relationshipName, relationships);

        #endregion

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
            ICreateService<TResource, int> create = null,
            IGetAllService<TResource, int> getAll = null,
            IGetByIdService<TResource, int> getById = null,
            IGetSecondaryService<TResource, int> getSecondary = null,
            IUpdateService<TResource, int> update = null,
            IDeleteService<TResource, int> delete = null,
            IAddRelationshipService<TResource, int> addRelationship = null,
            IGetRelationshipService<TResource, int> getRelationship = null,
            ISetRelationshipService<TResource, int> setRelationship = null,
            IDeleteRelationshipService<TResource, int> deleteRelationship = null)
            : base(options, loggerFactory, create, getAll, getById, getSecondary, update, delete, addRelationship, 
                getRelationship, setRelationship, deleteRelationship)
        { }
    }
}
