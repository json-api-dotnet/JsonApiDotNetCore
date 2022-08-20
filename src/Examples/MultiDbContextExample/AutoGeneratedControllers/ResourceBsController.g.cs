using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using MultiDbContextExample.Models;

namespace MultiDbContextExample.Controllers;

public sealed partial class ResourceBsController : JsonApiController<ResourceB, int>
{
    public ResourceBsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<ResourceB, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
