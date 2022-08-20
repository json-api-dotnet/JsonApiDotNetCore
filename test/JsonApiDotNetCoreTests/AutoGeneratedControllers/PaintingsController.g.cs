using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.HostingInIIS;

namespace JsonApiDotNetCoreTests.IntegrationTests.HostingInIIS;

public sealed partial class PaintingsController : JsonApiController<Painting, int>
{
    public PaintingsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Painting, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
