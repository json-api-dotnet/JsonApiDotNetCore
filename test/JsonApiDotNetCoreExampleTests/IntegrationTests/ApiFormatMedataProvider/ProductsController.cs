using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ApiFormatMedataProvider
{
    public sealed class ProductsController : JsonApiController<Product>
    {
        public ProductsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Product> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
