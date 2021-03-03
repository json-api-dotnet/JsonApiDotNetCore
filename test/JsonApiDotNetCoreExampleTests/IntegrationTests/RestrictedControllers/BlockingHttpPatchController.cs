using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RestrictedControllers
{
    [NoHttpPatch]
    public sealed class BlockingHttpPatchController : JsonApiController<Chair>
    {
        public BlockingHttpPatchController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Chair> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
