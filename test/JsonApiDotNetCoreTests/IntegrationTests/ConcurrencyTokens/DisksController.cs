using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.ConcurrencyTokens
{
    public sealed class DisksController : JsonApiController<Disk, long>
    {
        public DisksController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IResourceService<Disk, long> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
