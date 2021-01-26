using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RestrictedControllers
{
    [NoHttpDelete]
    public sealed class BlockingHttpDeleteController : JsonApiController<Sofa>
    {
        public BlockingHttpDeleteController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Sofa, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
