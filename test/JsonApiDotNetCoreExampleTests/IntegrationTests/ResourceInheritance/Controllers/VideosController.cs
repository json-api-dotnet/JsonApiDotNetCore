using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Controllers
{
    public sealed class VideosController : JsonApiController<Video>
    {
        public VideosController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Video> resourceService)
            : base(options, loggerFactory, resourceService) { }
    }
}
