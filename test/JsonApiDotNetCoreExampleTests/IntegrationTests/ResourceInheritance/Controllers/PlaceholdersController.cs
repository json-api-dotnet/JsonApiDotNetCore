using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    public sealed class PlaceholdersController : JsonApiController<Placeholder>
    {
        public PlaceholdersController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Placeholder> resourceService)
            : base(options, loggerFactory, resourceService) { }
    }
}
