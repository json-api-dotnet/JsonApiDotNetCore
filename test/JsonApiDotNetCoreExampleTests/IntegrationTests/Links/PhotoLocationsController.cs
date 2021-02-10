using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Links
{
    public sealed class PhotoLocationsController : JsonApiController<PhotoLocation>
    {
        public PhotoLocationsController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<PhotoLocation> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
