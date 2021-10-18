#nullable disable

using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers
{
    [HttpReadOnly]
    [DisableQueryString("skipCache")]
    public sealed class BlockingWritesController : JsonApiController<Bed, int>
    {
        public BlockingWritesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IResourceService<Bed, int> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
