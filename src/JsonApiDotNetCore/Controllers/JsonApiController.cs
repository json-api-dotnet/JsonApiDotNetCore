using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Controllers
{
    public class JsonApiController<T> : JsonApiController<T, int> where T : class, IIdentifiable<int>
    {
        private IJsonApiOptions jsonApiOptions;
        private IJsonApiContext jsonApiContext;
        private IResourceService<T> resourceService;
        private ILoggerFactory loggerFactory;

        public JsonApiController(
            IJsonApiOptions jsonApiOptions,
            IJsonApiContext jsonApiContext,
            IResourceService<T, int> resourceService,
            ILoggerFactory loggerFactory)
            : base(jsonApiOptions, jsonApiContext, resourceService, loggerFactory)
        { }

        public JsonApiController(
            IJsonApiOptions jsonApiOptions,
            IJsonApiContext jsonApiContext,
            IResourceService<T, int> resourceService)
            : base(jsonApiOptions, jsonApiContext, resourceService)
        { }

        public JsonApiController(
            IJsonApiOptions jsonApiOptions,
            IJsonApiContext jsonApiContext,
            IGetAllService<T, int> getAll = null,
            IGetByIdService<T, int> getById = null,
            IGetRelationshipService<T, int> getRelationship = null,
            IGetRelationshipsService<T, int> getRelationships = null,
            ICreateService<T, int> create = null,
            IUpdateService<T, int> update = null,
            IUpdateRelationshipService<T, int> updateRelationships = null,
            IDeleteService<T, int> delete = null
        ) : base(jsonApiOptions, jsonApiContext, getAll, getById, getRelationship, getRelationships, create, update, updateRelationships, delete) { }

        public JsonApiController(IJsonApiOptions jsonApiOptions,
                                 IJsonApiContext jsonApiContext,
                                 IResourceService<T> resourceService,
                                 ILoggerFactory loggerFactory)
            : base(jsonApiOptions, jsonApiContext, resourceService, loggerFactory)
        {
        }
    }

    public class JsonApiController<T, TId> : BaseJsonApiController<T, TId> where T : class, IIdentifiable<TId>
    {
        public JsonApiController(
            IJsonApiOptions jsonApiOptions,
            IJsonApiContext jsonApiContext,
            IResourceService<T, TId> resourceService,
            ILoggerFactory loggerFactory)
        : base(jsonApiOptions, jsonApiContext, resourceService, resourceService)
        { }

        public JsonApiController(
            IJsonApiOptions jsonApiOptions,
            IJsonApiContext jsonApiContext,
            IResourceService<T, TId> resourceService)
        : base(jsonApiOptions, jsonApiContext, resourceService)
        { }

        public JsonApiController(
            IJsonApiOptions jsonApiOptions,
            IJsonApiContext jsonApiContext,
            IGetAllService<T, TId> getAll = null,
            IGetByIdService<T, TId> getById = null,
            IGetRelationshipService<T, TId> getRelationship = null,
            IGetRelationshipsService<T, TId> getRelationships = null,
            ICreateService<T, TId> create = null,
            IUpdateService<T, TId> update = null,
            IUpdateRelationshipService<T, TId> updateRelationships = null,
            IDeleteService<T, TId> delete = null
        ) : base(jsonApiOptions, jsonApiContext, getAll, getById, getRelationship, getRelationships, create, update, updateRelationships, delete) { }

        [HttpGet]
        public override async Task<IActionResult> GetAsync() => await base.GetAsync();

        [HttpGet("{id}")]
        public override async Task<IActionResult> GetAsync(TId id) => await base.GetAsync(id);

        [HttpGet("{id}/relationships/{relationshipName}")]
        public override async Task<IActionResult> GetRelationshipsAsync(TId id, string relationshipName)
            => await base.GetRelationshipsAsync(id, relationshipName);

        [HttpGet("{id}/{relationshipName}")]
        public override async Task<IActionResult> GetRelationshipAsync(TId id, string relationshipName)
            => await base.GetRelationshipAsync(id, relationshipName);

        [HttpPost]
        public override async Task<IActionResult> PostAsync([FromBody] T entity)
            => await base.PostAsync(entity);

        [HttpPatch("{id}")]
        public override async Task<IActionResult> PatchAsync(TId id, [FromBody] T entity)
        {
            return await base.PatchAsync(id, entity);
        }

        [HttpPatch("{id}/relationships/{relationshipName}")]
        public override async Task<IActionResult> PatchRelationshipsAsync(
            TId id, string relationshipName, [FromBody] List<ResourceObject> relationships)
            => await base.PatchRelationshipsAsync(id, relationshipName, relationships);

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(TId id) => await base.DeleteAsync(id);
    }
}
