using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Filtering
{
    public sealed class FilterableResourcesController : JsonApiController<FilterableResource>
    {
        public FilterableResourcesController(IJsonApiOptions jsonApiOptions, ILoggerFactory loggerFactory,
            IResourceService<FilterableResource> resourceService)
            : base(jsonApiOptions, loggerFactory, resourceService)
        {
        }
    }
}
