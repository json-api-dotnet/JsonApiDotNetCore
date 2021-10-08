using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.Meta
{
    public sealed class ProductFamiliesController : JsonApiController<ProductFamily, int>
    {
        public ProductFamiliesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<ProductFamily, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
