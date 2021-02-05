using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings
{
    public sealed class WebAccountsController : JsonApiController<WebAccount>
    {
        public WebAccountsController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<WebAccount> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
