using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings.Filtering
{
    public sealed class FilterableResourcesController : JsonApiController<FilterableResource>
    {
        public FilterableResourcesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<FilterableResource> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
