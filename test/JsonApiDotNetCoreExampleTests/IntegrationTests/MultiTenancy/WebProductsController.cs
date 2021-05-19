using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.MultiTenancy
{
    public sealed class WebProductsController : JsonApiController<WebProduct>
    {
        public WebProductsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<WebProduct> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
