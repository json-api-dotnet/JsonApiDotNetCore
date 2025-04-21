using System.ComponentModel.DataAnnotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

#pragma warning disable format

namespace JsonApiDotNetCoreTests.IntegrationTests.IdObfuscation;

public abstract class ObfuscatedIdentifiableController<TResource>(
    IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<TResource, int> resourceService)
    : BaseJsonApiController<TResource, int>(options, resourceGraph, loggerFactory, resourceService)
    where TResource : class, IIdentifiable<int>
{
    private readonly HexadecimalCodec _codec = new();

    [HttpGet]
    [HttpHead]
    public override Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        return base.GetAsync(cancellationToken);
    }

    [HttpGet("{id}")]
    [HttpHead("{id}")]
    public Task<IActionResult> GetAsync([Required] string id, CancellationToken cancellationToken)
    {
        int idValue = _codec.Decode(id);
        return base.GetAsync(idValue, cancellationToken);
    }

    [HttpGet("{id}/{relationshipName}")]
    [HttpHead("{id}/{relationshipName}")]
    public Task<IActionResult> GetSecondaryAsync([Required] string id, [Required] [PreserveEmptyString] string relationshipName,
        CancellationToken cancellationToken)
    {
        int idValue = _codec.Decode(id);
        return base.GetSecondaryAsync(idValue, relationshipName, cancellationToken);
    }

    [HttpGet("{id}/relationships/{relationshipName}")]
    [HttpHead("{id}/relationships/{relationshipName}")]
    public Task<IActionResult> GetRelationshipAsync([Required] string id, [Required] [PreserveEmptyString] string relationshipName,
        CancellationToken cancellationToken)
    {
        int idValue = _codec.Decode(id);
        return base.GetRelationshipAsync(idValue, relationshipName, cancellationToken);
    }

    [HttpPost]
    public override Task<IActionResult> PostAsync([FromBody] [Required] TResource resource, CancellationToken cancellationToken)
    {
        return base.PostAsync(resource, cancellationToken);
    }

    [HttpPost("{id}/relationships/{relationshipName}")]
    public Task<IActionResult> PostRelationshipAsync([Required] string id, [Required] [PreserveEmptyString] string relationshipName,
        [FromBody] [Required] ISet<IIdentifiable> rightResourceIds, CancellationToken cancellationToken)
    {
        int idValue = _codec.Decode(id);
        return base.PostRelationshipAsync(idValue, relationshipName, rightResourceIds, cancellationToken);
    }

    [HttpPatch("{id}")]
    public Task<IActionResult> PatchAsync([Required] string id, [FromBody] [Required] TResource resource, CancellationToken cancellationToken)
    {
        int idValue = _codec.Decode(id);
        return base.PatchAsync(idValue, resource, cancellationToken);
    }

    [HttpPatch("{id}/relationships/{relationshipName}")]
    // Parameter `[Required] object? rightValue` makes Swashbuckle generate the OpenAPI request body as required. We don't actually validate ModelState, so it doesn't hurt.
    public Task<IActionResult> PatchRelationshipAsync([Required] string id, [Required] [PreserveEmptyString] string relationshipName,
        [FromBody] [Required] object? rightValue, CancellationToken cancellationToken)
    {
        int idValue = _codec.Decode(id);
        return base.PatchRelationshipAsync(idValue, relationshipName, rightValue, cancellationToken);
    }

    [HttpDelete("{id}")]
    public Task<IActionResult> DeleteAsync([Required] string id, CancellationToken cancellationToken)
    {
        int idValue = _codec.Decode(id);
        return base.DeleteAsync(idValue, cancellationToken);
    }

    [HttpDelete("{id}/relationships/{relationshipName}")]
    public Task<IActionResult> DeleteRelationshipAsync([Required] string id, [Required] [PreserveEmptyString] string relationshipName,
        [FromBody] [Required] ISet<IIdentifiable> rightResourceIds, CancellationToken cancellationToken)
    {
        int idValue = _codec.Decode(id);
        return base.DeleteRelationshipAsync(idValue, relationshipName, rightResourceIds, cancellationToken);
    }
}
