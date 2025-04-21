using System.ComponentModel.DataAnnotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace OpenApiTests.OpenApiGenerationFailures.MissingFromBody;

public sealed class MissingFromBodyOnPatchController(
    IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<RecycleBin, long> resourceService)
    : BaseJsonApiController<RecycleBin, long>(options, resourceGraph, loggerFactory, resourceService)
{
    // Not overriding the base method, to trigger the error that [FromBody] is missing.
    [HttpPatch("{id}")]
    public Task<IActionResult> AlternatePatchAsync([Required] long id, [Required] RecycleBin resource, CancellationToken cancellationToken)
    {
        return PatchAsync(id, resource, cancellationToken);
    }
}
