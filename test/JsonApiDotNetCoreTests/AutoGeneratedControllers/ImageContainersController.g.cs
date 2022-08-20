using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.Blobs;

namespace JsonApiDotNetCoreTests.IntegrationTests.Blobs;

public sealed partial class ImageContainersController : JsonApiController<ImageContainer, long>
{
    public ImageContainersController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<ImageContainer, long> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
