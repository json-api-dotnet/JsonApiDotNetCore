using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.Links;

namespace JsonApiDotNetCoreTests.IntegrationTests.Links;

public sealed partial class PhotoAlbumsController : JsonApiController<PhotoAlbum, System.Guid>
{
    public PhotoAlbumsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<PhotoAlbum, System.Guid> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
