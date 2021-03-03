using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceDefinitions
{
    public sealed class CallableResourcesController : JsonApiController<CallableResource>
    {
        public CallableResourcesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<CallableResource> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
