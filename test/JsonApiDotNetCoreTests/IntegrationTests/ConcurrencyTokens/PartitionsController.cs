using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.ConcurrencyTokens
{
    public sealed class PartitionsController : JsonApiController<Partition, long>
    {
        public PartitionsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IResourceService<Partition, long> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
