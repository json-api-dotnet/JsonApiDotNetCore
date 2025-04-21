using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace OpenApiTests.OpenApiGenerationFailures.MissingFromBody;

public sealed class MissingFromBodyOnPostController(
    IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<RecycleBin, long> resourceService)
    : BaseJsonApiController<RecycleBin, long>(options, resourceGraph, loggerFactory, resourceService)
{
    // Not overriding the base method, to trigger the error that [FromBody] is missing.
    [HttpPost]
    public Task<IActionResult> AlternatePostAsync(RecycleBin resource, CancellationToken cancellationToken)
    {
        return PostAsync(resource, cancellationToken);
    }
}
