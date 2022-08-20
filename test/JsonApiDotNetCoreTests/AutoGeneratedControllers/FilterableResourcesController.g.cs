using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Filtering;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.Filtering;

public sealed partial class FilterableResourcesController : JsonApiController<FilterableResource, int>
{
    public FilterableResourcesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<FilterableResource, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
