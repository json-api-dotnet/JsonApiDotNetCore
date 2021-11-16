using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Controllers;

/// <summary>
/// Implements the foundational ASP.NET controller layer in the JsonApiDotNetCore architecture that delegates to a Resource Service.
/// </summary>
/// <typeparam name="TResource">
/// The resource type.
/// </typeparam>
/// <typeparam name="TId">
/// The resource identifier type.
/// </typeparam>
public abstract class BaseJsonApiController<TResource, TId> : CoreJsonApiController
    where TResource : class, IIdentifiable<TId>
{
    private readonly IJsonApiOptions _options;
    private readonly IResourceGraph _resourceGraph;
    private readonly IGetAllService<TResource, TId>? _getAll;
    private readonly IGetByIdService<TResource, TId>? _getById;
    private readonly IGetSecondaryService<TResource, TId>? _getSecondary;
    private readonly IGetRelationshipService<TResource, TId>? _getRelationship;
    private readonly ICreateService<TResource, TId>? _create;
    private readonly IAddToRelationshipService<TResource, TId>? _addToRelationship;
    private readonly IUpdateService<TResource, TId>? _update;
    private readonly ISetRelationshipService<TResource, TId>? _setRelationship;
    private readonly IDeleteService<TResource, TId>? _delete;
    private readonly IRemoveFromRelationshipService<TResource, TId>? _removeFromRelationship;
    private readonly TraceLogWriter<BaseJsonApiController<TResource, TId>> _traceWriter;

    /// <summary>
    /// Creates an instance from a read/write service.
    /// </summary>
    protected BaseJsonApiController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<TResource, TId> resourceService)
        : this(options, resourceGraph, loggerFactory, resourceService, resourceService)
    {
    }

    /// <summary>
    /// Creates an instance from separate services for reading and writing.
    /// </summary>
    protected BaseJsonApiController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceQueryService<TResource, TId>? queryService = null, IResourceCommandService<TResource, TId>? commandService = null)
        : this(options, resourceGraph, loggerFactory, queryService, queryService, queryService, queryService, commandService, commandService, commandService,
            commandService, commandService, commandService)
    {
    }

    /// <summary>
    /// Creates an instance from separate services for the various individual read and write methods.
    /// </summary>
    protected BaseJsonApiController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IGetAllService<TResource, TId>? getAll = null, IGetByIdService<TResource, TId>? getById = null,
        IGetSecondaryService<TResource, TId>? getSecondary = null, IGetRelationshipService<TResource, TId>? getRelationship = null,
        ICreateService<TResource, TId>? create = null, IAddToRelationshipService<TResource, TId>? addToRelationship = null,
        IUpdateService<TResource, TId>? update = null, ISetRelationshipService<TResource, TId>? setRelationship = null,
        IDeleteService<TResource, TId>? delete = null, IRemoveFromRelationshipService<TResource, TId>? removeFromRelationship = null)
    {
        ArgumentGuard.NotNull(options);
        ArgumentGuard.NotNull(resourceGraph);
        ArgumentGuard.NotNull(loggerFactory);

        _options = options;
        _resourceGraph = resourceGraph;
        _traceWriter = new TraceLogWriter<BaseJsonApiController<TResource, TId>>(loggerFactory);
        _getAll = getAll;
        _getById = getById;
        _getSecondary = getSecondary;
        _getRelationship = getRelationship;
        _create = create;
        _addToRelationship = addToRelationship;
        _update = update;
        _setRelationship = setRelationship;
        _delete = delete;
        _removeFromRelationship = removeFromRelationship;
    }

    /// <summary>
    /// Gets a collection of primary resources. Example: <code><![CDATA[
    /// GET /articles HTTP/1.1
    /// ]]></code>
    /// </summary>
    public virtual async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        _traceWriter.LogMethodStart();

        if (_getAll == null)
        {
            throw new RouteNotAvailableException(HttpMethod.Get, Request.Path);
        }

        IReadOnlyCollection<TResource> resources = await _getAll.GetAsync(cancellationToken);

        return Ok(resources);
    }

    /// <summary>
    /// Gets a single primary resource by ID. Example: <code><![CDATA[
    /// GET /articles/1 HTTP/1.1
    /// ]]></code>
    /// </summary>
    public virtual async Task<IActionResult> GetAsync(TId id, CancellationToken cancellationToken)
    {
        _traceWriter.LogMethodStart(new
        {
            id
        });

        if (_getById == null)
        {
            throw new RouteNotAvailableException(HttpMethod.Get, Request.Path);
        }

        TResource resource = await _getById.GetAsync(id, cancellationToken);

        return Ok(resource);
    }

    /// <summary>
    /// Gets a secondary resource or collection of secondary resources. Example: <code><![CDATA[
    /// GET /articles/1/author HTTP/1.1
    /// ]]></code> Example:
    /// <code><![CDATA[
    /// GET /articles/1/revisions HTTP/1.1
    /// ]]></code>
    /// </summary>
    public virtual async Task<IActionResult> GetSecondaryAsync(TId id, string relationshipName, CancellationToken cancellationToken)
    {
        _traceWriter.LogMethodStart(new
        {
            id,
            relationshipName
        });

        ArgumentGuard.NotNullNorEmpty(relationshipName);

        if (_getSecondary == null)
        {
            throw new RouteNotAvailableException(HttpMethod.Get, Request.Path);
        }

        object? rightValue = await _getSecondary.GetSecondaryAsync(id, relationshipName, cancellationToken);

        return Ok(rightValue);
    }

    /// <summary>
    /// Gets a relationship value, which can be a <c>null</c>, a single object or a collection. Example:
    /// <code><![CDATA[
    /// GET /articles/1/relationships/author HTTP/1.1
    /// ]]></code> Example:
    /// <code><![CDATA[
    /// GET /articles/1/relationships/revisions HTTP/1.1
    /// ]]></code>
    /// </summary>
    public virtual async Task<IActionResult> GetRelationshipAsync(TId id, string relationshipName, CancellationToken cancellationToken)
    {
        _traceWriter.LogMethodStart(new
        {
            id,
            relationshipName
        });

        ArgumentGuard.NotNullNorEmpty(relationshipName);

        if (_getRelationship == null)
        {
            throw new RouteNotAvailableException(HttpMethod.Get, Request.Path);
        }

        object? rightValue = await _getRelationship.GetRelationshipAsync(id, relationshipName, cancellationToken);

        return Ok(rightValue);
    }

    /// <summary>
    /// Creates a new resource with attributes, relationships or both. Example: <code><![CDATA[
    /// POST /articles HTTP/1.1
    /// ]]></code>
    /// </summary>
    public virtual async Task<IActionResult> PostAsync([FromBody] TResource resource, CancellationToken cancellationToken)
    {
        _traceWriter.LogMethodStart(new
        {
            resource
        });

        ArgumentGuard.NotNull(resource);

        if (_create == null)
        {
            throw new RouteNotAvailableException(HttpMethod.Post, Request.Path);
        }

        if (_options.ValidateModelState && !ModelState.IsValid)
        {
            throw new InvalidModelStateException(ModelState, typeof(TResource), _options.IncludeExceptionStackTraceInErrors, _resourceGraph);
        }

        TResource? newResource = await _create.CreateAsync(resource, cancellationToken);

        TResource resultResource = newResource ?? resource;
        string? resourceVersion = resultResource.GetVersion();
        string locationUrl = $"{HttpContext.Request.Path}/{resultResource.StringId}{(resourceVersion != null ? $";v~{resourceVersion}" : null)}";

        if (newResource == null)
        {
            HttpContext.Response.Headers["Location"] = locationUrl;
            return NoContent();
        }

        return Created(locationUrl, newResource);
    }

    /// <summary>
    /// Adds resources to a to-many relationship. Example: <code><![CDATA[
    /// POST /articles/1/revisions HTTP/1.1
    /// ]]></code> Example:
    /// <code><![CDATA[
    /// POST /articles/1;v~8/revisions HTTP/1.1
    /// ]]></code>
    /// </summary>
    /// <param name="id">
    /// Identifies the left side of the relationship.
    /// </param>
    /// <param name="relationshipName">
    /// The relationship to add resources to.
    /// </param>
    /// <param name="rightResourceIds">
    /// The set of resources to add to the relationship.
    /// </param>
    /// <param name="cancellationToken">
    /// Propagates notification that request handling should be canceled.
    /// </param>
    public virtual async Task<IActionResult> PostRelationshipAsync(TId id, string relationshipName, [FromBody] ISet<IIdentifiable> rightResourceIds,
        CancellationToken cancellationToken)
    {
        _traceWriter.LogMethodStart(new
        {
            id,
            relationshipName,
            rightResourceIds
        });

        ArgumentGuard.NotNullNorEmpty(relationshipName);
        ArgumentGuard.NotNull(rightResourceIds);

        if (_addToRelationship == null)
        {
            throw new RouteNotAvailableException(HttpMethod.Post, Request.Path);
        }

        await _addToRelationship.AddToToManyRelationshipAsync(id, relationshipName, rightResourceIds, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Updates the attributes and/or relationships of an existing resource. Only the values of sent attributes are replaced. And only the values of sent
    /// relationships are replaced. Example: <code><![CDATA[
    /// PATCH /articles/1 HTTP/1.1
    /// ]]></code> Example:
    /// <code><![CDATA[
    /// PATCH /articles/1;v~8 HTTP/1.1
    /// ]]></code>
    /// </summary>
    public virtual async Task<IActionResult> PatchAsync(TId id, [FromBody] TResource resource, CancellationToken cancellationToken)
    {
        _traceWriter.LogMethodStart(new
        {
            id,
            resource
        });

        ArgumentGuard.NotNull(resource);

        if (_update == null)
        {
            throw new RouteNotAvailableException(HttpMethod.Patch, Request.Path);
        }

        if (_options.ValidateModelState && !ModelState.IsValid)
        {
            throw new InvalidModelStateException(ModelState, typeof(TResource), _options.IncludeExceptionStackTraceInErrors, _resourceGraph);
        }

        TResource? updated = await _update.UpdateAsync(id, resource, cancellationToken);

        return updated == null ? NoContent() : Ok(updated);
    }

    /// <summary>
    /// Performs a complete replacement of a relationship on an existing resource. Example:
    /// <code><![CDATA[
    /// PATCH /articles/1/relationships/author HTTP/1.1
    /// ]]></code> Example:
    /// <code><![CDATA[
    /// PATCH /articles/1;v~8/relationships/author HTTP/1.1
    /// ]]></code> Example:
    /// <code><![CDATA[
    /// PATCH /articles/1/relationships/revisions HTTP/1.1
    /// ]]></code> Example:
    /// <code><![CDATA[
    /// PATCH /articles/1;v~8/relationships/revisions HTTP/1.1
    /// ]]></code>
    /// </summary>
    /// <param name="id">
    /// Identifies the left side of the relationship.
    /// </param>
    /// <param name="relationshipName">
    /// The relationship for which to perform a complete replacement.
    /// </param>
    /// <param name="rightValue">
    /// The resource or set of resources to assign to the relationship.
    /// </param>
    /// <param name="cancellationToken">
    /// Propagates notification that request handling should be canceled.
    /// </param>
    public virtual async Task<IActionResult> PatchRelationshipAsync(TId id, string relationshipName, [FromBody] object? rightValue,
        CancellationToken cancellationToken)
    {
        _traceWriter.LogMethodStart(new
        {
            id,
            relationshipName,
            rightValue
        });

        ArgumentGuard.NotNullNorEmpty(relationshipName);

        if (_setRelationship == null)
        {
            throw new RouteNotAvailableException(HttpMethod.Patch, Request.Path);
        }

        await _setRelationship.SetRelationshipAsync(id, relationshipName, rightValue, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deletes an existing resource. Example: <code><![CDATA[
    /// DELETE /articles/1 HTTP/1.1
    /// ]]></code> Example:
    /// <code><![CDATA[
    /// DELETE /articles/1;v~8 HTTP/1.1
    /// ]]></code>
    /// </summary>
    public virtual async Task<IActionResult> DeleteAsync(TId id, CancellationToken cancellationToken)
    {
        _traceWriter.LogMethodStart(new
        {
            id
        });

        if (_delete == null)
        {
            throw new RouteNotAvailableException(HttpMethod.Delete, Request.Path);
        }

        await _delete.DeleteAsync(id, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Removes resources from a to-many relationship. Example: <code><![CDATA[
    /// DELETE /articles/1/relationships/revisions HTTP/1.1
    /// ]]></code> Example:
    /// <code><![CDATA[
    /// DELETE /articles/1;v~8/relationships/revisions HTTP/1.1
    /// ]]></code>
    /// </summary>
    /// <param name="id">
    /// Identifies the left side of the relationship.
    /// </param>
    /// <param name="relationshipName">
    /// The relationship to remove resources from.
    /// </param>
    /// <param name="rightResourceIds">
    /// The set of resources to remove from the relationship.
    /// </param>
    /// <param name="cancellationToken">
    /// Propagates notification that request handling should be canceled.
    /// </param>
    public virtual async Task<IActionResult> DeleteRelationshipAsync(TId id, string relationshipName, [FromBody] ISet<IIdentifiable> rightResourceIds,
        CancellationToken cancellationToken)
    {
        _traceWriter.LogMethodStart(new
        {
            id,
            relationshipName,
            rightResourceIds
        });

        ArgumentGuard.NotNullNorEmpty(relationshipName);
        ArgumentGuard.NotNull(rightResourceIds);

        if (_removeFromRelationship == null)
        {
            throw new RouteNotAvailableException(HttpMethod.Delete, Request.Path);
        }

        await _removeFromRelationship.RemoveFromToManyRelationshipAsync(id, relationshipName, rightResourceIds, cancellationToken);

        return NoContent();
    }
}
