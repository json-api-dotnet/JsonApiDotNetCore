using System;
using System.Reflection;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Controllers
{
    public class BaseJsonApiController<T, TId>
        : JsonApiControllerMixin
        where T : class, IIdentifiable<TId>
    {
        private readonly IGetAllService<T, TId> _getAll;
        private readonly IGetByIdService<T, TId> _getById;
        private readonly IGetRelationshipService<T, TId> _getRelationship;
        private readonly IGetRelationshipsService<T, TId> _getRelationships;
        private readonly ICreateService<T, TId> _create;
        private readonly IUpdateService<T, TId> _update;
        private readonly IUpdateRelationshipService<T, TId> _updateRelationships;
        private readonly IDeleteService<T, TId> _delete;
        private readonly ILogger<BaseJsonApiController<T, TId>> _logger;
        private readonly IJsonApiOptions _jsonApiOptions;
        
        public BaseJsonApiController(
            IJsonApiOptions jsonApiOptions,
            IResourceService<T, TId> resourceService,
            ILoggerFactory loggerFactory)
        {
            if (loggerFactory != null)
                _logger = loggerFactory.CreateLogger<BaseJsonApiController<T, TId>>();
            else
                _logger = new Logger<BaseJsonApiController<T, TId>>(new LoggerFactory());

            _jsonApiOptions = jsonApiOptions;
            _getAll = resourceService;
            _getById = resourceService;
            _getRelationship = resourceService;
            _getRelationships = resourceService;
            _create = resourceService;
            _update = resourceService;
            _updateRelationships = resourceService;
            _delete = resourceService;
        }

        public BaseJsonApiController(
            IJsonApiOptions jsonApiOptions,
            IResourceQueryService<T, TId> queryService = null,
            IResourceCmdService<T, TId> cmdService = null)
        {
            _jsonApiOptions = jsonApiOptions;
            _getAll = queryService;
            _getById = queryService;
            _getRelationship = queryService;
            _getRelationships = queryService;
            _create = cmdService;
            _update = cmdService;
            _updateRelationships = cmdService;
            _delete = cmdService;
        }

        /// <param name="jsonApiOptions"></param>
        /// <param name="getAll"></param>
        /// <param name="getById"></param>
        /// <param name="getRelationship"></param>
        /// <param name="getRelationships"></param>
        /// <param name="create"></param>
        /// <param name="update"></param>
        /// <param name="updateRelationships"></param>
        /// <param name="delete"></param>
        public BaseJsonApiController(
            IJsonApiOptions jsonApiOptions,
            IGetAllService<T, TId> getAll = null,
            IGetByIdService<T, TId> getById = null,
            IGetRelationshipService<T, TId> getRelationship = null,
            IGetRelationshipsService<T, TId> getRelationships = null,
            ICreateService<T, TId> create = null,
            IUpdateService<T, TId> update = null,
            IUpdateRelationshipService<T, TId> updateRelationships = null,
            IDeleteService<T, TId> delete = null)
        {
            _jsonApiOptions = jsonApiOptions;
            _getAll = getAll;
            _getById = getById;
            _getRelationship = getRelationship;
            _getRelationships = getRelationships;
            _create = create;
            _update = update;
            _updateRelationships = updateRelationships;
            _delete = delete;
        }

        public virtual async Task<IActionResult> GetAsync()
        {
            if (_getAll == null) throw Exceptions.UnSupportedRequestMethod;
            var entities = await _getAll.GetAsync();
            return Ok(entities);
        }

        public virtual async Task<IActionResult> GetAsync(TId id)
        {
            if (_getById == null) throw Exceptions.UnSupportedRequestMethod;
            var entity = await _getById.GetAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(entity);
        }

        public virtual async Task<IActionResult> GetRelationshipsAsync(TId id, string relationshipName)
        {
            if (_getRelationships == null)
                throw Exceptions.UnSupportedRequestMethod;
            var relationship = await _getRelationships.GetRelationshipsAsync(id, relationshipName);
            if (relationship == null)
                return NotFound();

            return Ok(relationship);
        }

        public virtual async Task<IActionResult> GetRelationshipAsync(TId id, string relationshipName)
        {
            if (_getRelationship == null) throw Exceptions.UnSupportedRequestMethod;
            var relationship = await _getRelationship.GetRelationshipAsync(id, relationshipName);
            return Ok(relationship);
        }

        public virtual async Task<IActionResult> PostAsync([FromBody] T entity)
        {
            if (_create == null)
                throw Exceptions.UnSupportedRequestMethod;

            if (entity == null)
                return UnprocessableEntity();

            if (!_jsonApiOptions.AllowClientGeneratedIds && !string.IsNullOrEmpty(entity.StringId))
                return Forbidden();

            if (_jsonApiOptions.ValidateModelState && !ModelState.IsValid)
                return UnprocessableEntity(ModelState.ConvertToErrorCollection<T>());

            entity = await _create.CreateAsync(entity);

            return Created($"{HttpContext.Request.Path}/{entity.Id}", entity);
        }

        public virtual async Task<IActionResult> PatchAsync(TId id, [FromBody] T entity)
        {
            if (_update == null) throw Exceptions.UnSupportedRequestMethod;
            if (entity == null)
                return UnprocessableEntity();

            if (_jsonApiOptions.ValidateModelState && !ModelState.IsValid)
                return UnprocessableEntity(ModelState.ConvertToErrorCollection<T>());

            var updatedEntity = await _update.UpdateAsync(id, entity);

            if (updatedEntity == null)
                return NotFound();

            return Ok(updatedEntity);
        }

        public virtual async Task<IActionResult> PatchRelationshipsAsync(TId id, string relationshipName, [FromBody] object relationships)
        {
            if (_updateRelationships == null) throw Exceptions.UnSupportedRequestMethod;
            await _updateRelationships.UpdateRelationshipsAsync(id, relationshipName, relationships);
            return Ok();
        }

        public virtual async Task<IActionResult> DeleteAsync(TId id)
        {
            if (_delete == null) throw Exceptions.UnSupportedRequestMethod;
            var wasDeleted = await _delete.DeleteAsync(id);
            if (!wasDeleted)
                return NotFound();
            return NoContent();
        }

        //internal Type GetAssociatedResource()
        //{
        //    return GetType().GetMethod(nameof(GetAssociatedResource), BindingFlags.Instance | BindingFlags.NonPublic)
        //                    .DeclaringType
        //                    .GetGenericArguments()[0];
        //}
    }
    public class BaseJsonApiController<T>
    : BaseJsonApiController<T, int>
    where T : class, IIdentifiable<int>
    {
        public BaseJsonApiController(
            IJsonApiOptions jsonApiOptions,
            IResourceService<T, int> resourceService
        ) : base(jsonApiOptions, resourceService, resourceService) { }

        public BaseJsonApiController(
            IJsonApiOptions jsonApiOptions,
            IResourceQueryService<T, int> queryService = null,
            IResourceCmdService<T, int> cmdService = null
        ) : base(jsonApiOptions, queryService, cmdService) { }


        public BaseJsonApiController(
            IJsonApiOptions jsonApiOptions,
            IGetAllService<T, int> getAll = null,
            IGetByIdService<T, int> getById = null,
            IGetRelationshipService<T, int> getRelationship = null,
            IGetRelationshipsService<T, int> getRelationships = null,
            ICreateService<T, int> create = null,
            IUpdateService<T, int> update = null,
            IUpdateRelationshipService<T, int> updateRelationships = null,
            IDeleteService<T, int> delete = null
        ) : base(jsonApiOptions, getAll, getById, getRelationship, getRelationships, create, update, updateRelationships, delete) { }
    }
}
