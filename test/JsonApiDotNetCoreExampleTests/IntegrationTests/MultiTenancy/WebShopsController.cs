using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.MultiTenancy
{
    [DisableRoutingConvention]
    [Route("{countryCode}/shops")]
    public sealed class WebShopsController : JsonApiController<WebShop>
    {
        public WebShopsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<WebShop> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
