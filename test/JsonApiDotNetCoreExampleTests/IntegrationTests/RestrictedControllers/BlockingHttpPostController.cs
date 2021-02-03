using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RestrictedControllers
{
    [NoHttpPost]
    public sealed class BlockingHttpPostController : JsonApiController<Table>
    {
        public BlockingHttpPostController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Table> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
