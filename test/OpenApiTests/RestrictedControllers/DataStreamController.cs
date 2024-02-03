using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace OpenApiTests.RestrictedControllers;

public sealed class DataStreamController(
    IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<DataStream, long> resourceService)
    : BaseJsonApiController<DataStream, long>(options, resourceGraph, loggerFactory, resourceService)
{
    [HttpGet]
    [HttpHead]
    public override Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        return base.GetAsync(cancellationToken);
    }

    [HttpGet("{id}")]
    [HttpHead("{id}")]
    public override Task<IActionResult> GetAsync(long id, CancellationToken cancellationToken)
    {
        return base.GetAsync(id, cancellationToken);
    }
}
