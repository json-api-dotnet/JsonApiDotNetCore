using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.Meta;

namespace JsonApiDotNetCoreTests.IntegrationTests.Meta;

public sealed partial class ProductFamiliesController : JsonApiController<ProductFamily, int>
{
    public ProductFamiliesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<ProductFamily, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
