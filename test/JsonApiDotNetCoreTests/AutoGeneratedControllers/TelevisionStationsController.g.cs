using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.Archiving;

namespace JsonApiDotNetCoreTests.IntegrationTests.Archiving;

public sealed partial class TelevisionStationsController : JsonApiController<TelevisionStation, int>
{
    public TelevisionStationsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<TelevisionStation, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
