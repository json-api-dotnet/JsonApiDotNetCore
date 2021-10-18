#nullable disable

using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.IdObfuscation
{
    public sealed class DebitCardsController : ObfuscatedIdentifiableController<DebitCard>
    {
        public DebitCardsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IResourceService<DebitCard, int> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
