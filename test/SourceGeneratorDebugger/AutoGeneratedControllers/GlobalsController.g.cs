using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;

public sealed partial class GlobalsController : JsonApiController<Global, int>
{
    public GlobalsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Global, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
