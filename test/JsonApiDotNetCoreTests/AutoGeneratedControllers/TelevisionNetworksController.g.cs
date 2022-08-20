using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.Archiving;

namespace JsonApiDotNetCoreTests.IntegrationTests.Archiving;

public sealed partial class TelevisionNetworksController : JsonApiController<TelevisionNetwork, int>
{
    public TelevisionNetworksController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<TelevisionNetwork, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
