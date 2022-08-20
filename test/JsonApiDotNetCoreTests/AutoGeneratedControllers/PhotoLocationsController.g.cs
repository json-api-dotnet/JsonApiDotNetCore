using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.Links;

namespace JsonApiDotNetCoreTests.IntegrationTests.Links;

public sealed partial class PhotoLocationsController : JsonApiController<PhotoLocation, int>
{
    public PhotoLocationsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<PhotoLocation, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
