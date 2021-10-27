using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers
{
    [NoHttpPost]
    public sealed class BlockingHttpPostController : JsonApiController<Table, int>
    {
        public BlockingHttpPostController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Table, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
