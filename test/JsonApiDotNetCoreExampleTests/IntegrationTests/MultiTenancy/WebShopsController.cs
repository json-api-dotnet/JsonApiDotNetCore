using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.MultiTenancy
{
    public sealed class WebShopsController : JsonApiController<WebShop>
    {
        public WebShopsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<WebShop> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
