using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.HostingInIIS;

namespace JsonApiDotNetCoreTests.IntegrationTests.HostingInIIS;

public sealed partial class ArtGalleriesController : JsonApiController<ArtGallery, int>
{
    public ArtGalleriesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<ArtGallery, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
