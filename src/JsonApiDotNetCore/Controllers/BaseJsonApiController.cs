using System.Net.Http;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Controllers
{
    public abstract class BaseJsonApiController<TResource, TId> : CoreJsonApiController where TResource : class, IIdentifiable<TId>
    {
        private readonly IJsonApiOptions _options;
        private readonly IGetAllService<TResource, TId> _getAll;
        private readonly IGetByIdService<TResource, TId> _getById;
        private readonly IGetSecondaryService<TResource, TId> _getSecondary;
        private readonly IGetRelationshipService<TResource, TId> _getRelationship;
        private readonly ICreateService<TResource, TId> _create;
        private readonly IUpdateService<TResource, TId> _update;
        private readonly IUpdateRelationshipService<TResource, TId> _updateRelationships;
        private readonly IDeleteService<TResource, TId> _delete;
        private readonly ILogger<BaseJsonApiController<TResource, TId>> _logger;

        protected BaseJsonApiController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<TResource, TId> resourceService)
            : this(options, loggerFactory, resourceService, resourceService, resourceService, resourceService,
                resourceService, resourceService, resourceService, resourceService)
        { }

        protected BaseJsonApiController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceQueryService<TResource, TId> queryService = null,
            IResourceCommandService<TResource, TId> commandService = null)
            : this(options, loggerFactory, queryService, queryService, queryService, queryService, commandService,
                commandService, commandService, commandService)
        { }

        protected BaseJsonApiController(
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
        {
            _options = options;
            _logger = loggerFactory.CreateLogger<BaseJsonApiController<TResource, TId>>();
            _getAll = getAll;
            _getById = getById;
            _getSecondary = getSecondary;
            _getRelationship = getRelationship;
            _create = create;
            _update = update;
            _updateRelationships = updateRelationships;
            _delete = delete;
        }

        public virtual async Task<IActionResult> GetAsync()
        {
            _logger.LogTrace($"Entering {nameof(GetAsync)}().");

            if (_getAll == null) throw new RequestMethodNotAllowedException(HttpMethod.Get);
            var resources = await _getAll.GetAsync();
            return Ok(resources);
        }

        public virtual async Task<IActionResult> GetAsync(TId id)
        {
            _logger.LogTrace($"Entering {nameof(GetAsync)}('{id}').");

            if (_getById == null) throw new RequestMethodNotAllowedException(HttpMethod.Get);
            var resource = await _getById.GetAsync(id);
            return Ok(resource);
        }

        public virtual async Task<IActionResult> GetRelationshipAsync(TId id, string relationshipName)
        {
            _logger.LogTrace($"Entering {nameof(GetRelationshipAsync)}('{id}', '{relationshipName}').");

            if (_getRelationship == null) throw new RequestMethodNotAllowedException(HttpMethod.Get);
            var relationship = await _getRelationship.GetRelationshipAsync(id, relationshipName);

            return Ok(relationship);
        }

        public virtual async Task<IActionResult> GetSecondaryAsync(TId id, string relationshipName)
        {
            _logger.LogTrace($"Entering {nameof(GetSecondaryAsync)}('{id}', '{relationshipName}').");

            if (_getSecondary == null) throw new RequestMethodNotAllowedException(HttpMethod.Get);
            var relationship = await _getSecondary.GetSecondaryAsync(id, relationshipName);
            return Ok(relationship);
        }

        public virtual async Task<IActionResult> PostAsync([FromBody] TResource resource)
        {
            _logger.LogTrace($"Entering {nameof(PostAsync)}({(resource == null ? "null" : "object")}).");

            if (_create == null)
                throw new RequestMethodNotAllowedException(HttpMethod.Post);

            if (resource == null)
                throw new InvalidRequestBodyException(null, null, null);

            if (!_options.AllowClientGeneratedIds && !string.IsNullOrEmpty(resource.StringId))
                throw new ResourceIdInPostRequestNotAllowedException();

            if (_options.ValidateModelState && !ModelState.IsValid)
            {
                var namingStrategy = _options.SerializerContractResolver.NamingStrategy;
                throw new InvalidModelStateException(ModelState, typeof(TResource), _options.IncludeExceptionStackTraceInErrors, namingStrategy);
            }

            resource = await _create.CreateAsync(resource);

            return Created($"{HttpContext.Request.Path}/{resource.StringId}", resource);
        }

        public virtual async Task<IActionResult> PatchAsync(TId id, [FromBody] TResource resource)
        {
            _logger.LogTrace($"Entering {nameof(PatchAsync)}('{id}', {(resource == null ? "null" : "object")}).");

            if (_update == null) throw new RequestMethodNotAllowedException(HttpMethod.Patch);
            if (resource == null)
                throw new InvalidRequestBodyException(null, null, null);

            if (_options.ValidateModelState && !ModelState.IsValid)
            {
                var namingStrategy = _options.SerializerContractResolver.NamingStrategy;
                throw new InvalidModelStateException(ModelState, typeof(TResource), _options.IncludeExceptionStackTraceInErrors, namingStrategy);
            }

            var updated = await _update.UpdateAsync(id, resource);
            return updated == null ? Ok(null) : Ok(updated);
        }

        public virtual async Task<IActionResult> PatchRelationshipAsync(TId id, string relationshipName, [FromBody] object relationships)
        {
            _logger.LogTrace($"Entering {nameof(PatchRelationshipAsync)}('{id}', '{relationshipName}', {(relationships == null ? "null" : "object")}).");

            if (_updateRelationships == null) throw new RequestMethodNotAllowedException(HttpMethod.Patch);
            await _updateRelationships.UpdateRelationshipAsync(id, relationshipName, relationships);
            return Ok();
        }

        public virtual async Task<IActionResult> DeleteAsync(TId id)
        {
            _logger.LogTrace($"Entering {nameof(DeleteAsync)}('{id}).");

            if (_delete == null) throw new RequestMethodNotAllowedException(HttpMethod.Delete);
            await _delete.DeleteAsync(id);

            return NoContent();
        }
    }

    public abstract class BaseJsonApiController<TResource> : BaseJsonApiController<TResource, int> where TResource : class, IIdentifiable<int>
    {
        protected BaseJsonApiController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceService<TResource, int> resourceService)
            : base(options, loggerFactory, resourceService, resourceService)
        { }

        protected BaseJsonApiController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IResourceQueryService<TResource, int> queryService = null,
            IResourceCommandService<TResource, int> commandService = null)
            : base(options, loggerFactory, queryService, commandService)
        { }

        protected BaseJsonApiController(
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
