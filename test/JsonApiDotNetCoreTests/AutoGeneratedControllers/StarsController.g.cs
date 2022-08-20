using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

public sealed partial class StarsController : JsonApiController<Star, int>
{
    public StarsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Star, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
