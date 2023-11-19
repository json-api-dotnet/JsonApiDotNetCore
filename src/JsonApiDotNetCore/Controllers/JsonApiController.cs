using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Controllers;

/// <summary>
/// The base class to derive resource-specific controllers from. This class delegates all work to <see cref="BaseJsonApiController{TResource, TId}" />
/// but adds attributes for routing templates. If you want to provide routing templates yourself, you should derive from BaseJsonApiController directly.
/// </summary>
/// <typeparam name="TResource">
/// The resource type.
/// </typeparam>
/// <typeparam name="TId">
/// The resource identifier type.
/// </typeparam>
public abstract class JsonApiController<TResource, TId> : BaseJsonApiController<TResource, TId>
    where TResource : class, IIdentifiable<TId>
{
    /// <inheritdoc />
    protected JsonApiController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<TResource, TId> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }

    /// <inheritdoc />
    protected JsonApiController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IGetAllService<TResource, TId>? getAll = null, IGetByIdService<TResource, TId>? getById = null,
        IGetSecondaryService<TResource, TId>? getSecondary = null, IGetRelationshipService<TResource, TId>? getRelationship = null,
        ICreateService<TResource, TId>? create = null, IAddToRelationshipService<TResource, TId>? addToRelationship = null,
        IUpdateService<TResource, TId>? update = null, ISetRelationshipService<TResource, TId>? setRelationship = null,
        IDeleteService<TResource, TId>? delete = null, IRemoveFromRelationshipService<TResource, TId>? removeFromRelationship = null)
        : base(options, resourceGraph, loggerFactory, getAll, getById, getSecondary, getRelationship, create, addToRelationship, update, setRelationship,
            delete, removeFromRelationship)
    {
    }

    /// <inheritdoc />
    [HttpGet]
    [HttpHead]
    public override Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        return base.GetAsync(cancellationToken);
    }

    /// <inheritdoc />
    [HttpGet("{id}")]
    [HttpHead("{id}")]
    public override Task<IActionResult> GetAsync(TId id, CancellationToken cancellationToken)
    {
        return base.GetAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    [HttpGet("{id}/{relationshipName}")]
    [HttpHead("{id}/{relationshipName}")]
    public override Task<IActionResult> GetSecondaryAsync(TId id, string relationshipName, CancellationToken cancellationToken)
    {
        return base.GetSecondaryAsync(id, relationshipName, cancellationToken);
    }

    /// <inheritdoc />
    [HttpGet("{id}/relationships/{relationshipName}")]
    [HttpHead("{id}/relationships/{relationshipName}")]
    public override Task<IActionResult> GetRelationshipAsync(TId id, string relationshipName, CancellationToken cancellationToken)
    {
        return base.GetRelationshipAsync(id, relationshipName, cancellationToken);
    }

    /// <inheritdoc />
    [HttpPost]
    public override Task<IActionResult> PostAsync([FromBody] TResource resource, CancellationToken cancellationToken)
    {
        return base.PostAsync(resource, cancellationToken);
    }

    /// <inheritdoc />
    [HttpPost("{id}/relationships/{relationshipName}")]
    public override Task<IActionResult> PostRelationshipAsync(TId id, string relationshipName, [FromBody] ISet<IIdentifiable> rightResourceIds,
        CancellationToken cancellationToken)
    {
        return base.PostRelationshipAsync(id, relationshipName, rightResourceIds, cancellationToken);
    }

    /// <inheritdoc />
    [HttpPatch("{id}")]
    public override Task<IActionResult> PatchAsync(TId id, [FromBody] TResource resource, CancellationToken cancellationToken)
    {
        return base.PatchAsync(id, resource, cancellationToken);
    }

    /// <inheritdoc />
    [HttpPatch("{id}/relationships/{relationshipName}")]
    public override Task<IActionResult> PatchRelationshipAsync(TId id, string relationshipName, [FromBody] object? rightValue,
        CancellationToken cancellationToken)
    {
        return base.PatchRelationshipAsync(id, relationshipName, rightValue, cancellationToken);
    }

    /// <inheritdoc />
    [HttpDelete("{id}")]
    public override Task<IActionResult> DeleteAsync(TId id, CancellationToken cancellationToken)
    {
        return base.DeleteAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    [HttpDelete("{id}/relationships/{relationshipName}")]
    public override Task<IActionResult> DeleteRelationshipAsync(TId id, string relationshipName, [FromBody] ISet<IIdentifiable> rightResourceIds,
        CancellationToken cancellationToken)
    {
        return base.DeleteRelationshipAsync(id, relationshipName, rightResourceIds, cancellationToken);
    }
}
