using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Controllers
{
    public class JsonApiController<T, TId> : BaseJsonApiController<T, TId> where T : class, IIdentifiable<TId>
    {
        public JsonApiController(
            IJsonApiOptions jsonApiOptions,
            ILoggerFactory loggerFactory,
            IResourceService<T, TId> resourceService)
            : base(jsonApiOptions, loggerFactory, resourceService)
        { }

        public JsonApiController(
            IJsonApiOptions jsonApiOptions,
            ILoggerFactory loggerFactory,
            IGetAllService<T, TId> getAll = null,
            IGetByIdService<T, TId> getById = null,
            IGetSecondaryService<T, TId> getSecondary = null,
            IGetRelationshipService<T, TId> getRelationship = null,
            ICreateService<T, TId> create = null,
            IUpdateService<T, TId> update = null,
            IUpdateRelationshipService<T, TId> updateRelationships = null,
            IDeleteService<T, TId> delete = null)
            : base(jsonApiOptions, loggerFactory, getAll, getById, getSecondary, getRelationship, create, update,
                updateRelationships, delete)
        { }

        [HttpGet]
        public override async Task<IActionResult> GetAsync() => await base.GetAsync();

        [HttpGet("{id}")]
        public override async Task<IActionResult> GetAsync(TId id) => await base.GetAsync(id);

        [HttpGet("{id}/relationships/{relationshipName}")]
        public override async Task<IActionResult> GetRelationshipAsync(TId id, string relationshipName)
            => await base.GetRelationshipAsync(id, relationshipName);

        [HttpGet("{id}/{relationshipName}")]
        public override async Task<IActionResult> GetSecondaryAsync(TId id, string relationshipName)
            => await base.GetSecondaryAsync(id, relationshipName);

        [HttpPost]
        public override async Task<IActionResult> PostAsync([FromBody] T resource)
            => await base.PostAsync(resource);

        [HttpPatch("{id}")]
        public override async Task<IActionResult> PatchAsync(TId id, [FromBody] T resource)
        {
            return await base.PatchAsync(id, resource);
        }

        [HttpPatch("{id}/relationships/{relationshipName}")]
        public override async Task<IActionResult> PatchRelationshipAsync(
            TId id, string relationshipName, [FromBody] object relationships)
            => await base.PatchRelationshipAsync(id, relationshipName, relationships);

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(TId id) => await base.DeleteAsync(id);
    }

    public class JsonApiController<T> : JsonApiController<T, int> where T : class, IIdentifiable<int>
    {
        public JsonApiController(
            IJsonApiOptions jsonApiOptions,
            ILoggerFactory loggerFactory,
            IResourceService<T, int> resourceService)
            : base(jsonApiOptions, loggerFactory, resourceService)
        { }

        public JsonApiController(
            IJsonApiOptions jsonApiOptions,
            ILoggerFactory loggerFactory,
            IGetAllService<T, int> getAll = null,
            IGetByIdService<T, int> getById = null,
            IGetSecondaryService<T, int> getSecondary = null,
            IGetRelationshipService<T, int> getRelationship = null,
            ICreateService<T, int> create = null,
            IUpdateService<T, int> update = null,
            IUpdateRelationshipService<T, int> updateRelationships = null,
            IDeleteService<T, int> delete = null)
            : base(jsonApiOptions, loggerFactory, getAll, getById, getSecondary, getRelationship, create, update,
                updateRelationships, delete)
        { }
    }
}
