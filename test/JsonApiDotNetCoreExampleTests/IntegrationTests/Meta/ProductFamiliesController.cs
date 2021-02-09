using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Meta
{
    public sealed class ProductFamiliesController : JsonApiController<ProductFamily>
    {
        public ProductFamiliesController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<ProductFamily> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
