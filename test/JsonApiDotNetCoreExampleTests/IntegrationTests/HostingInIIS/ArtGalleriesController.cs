using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.HostingInIIS
{
    public sealed class ArtGalleriesController : JsonApiController<ArtGallery>
    {
        public ArtGalleriesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<ArtGallery> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
