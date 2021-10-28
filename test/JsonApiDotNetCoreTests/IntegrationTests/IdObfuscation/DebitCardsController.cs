using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.IdObfuscation
{
    public sealed class DebitCardsController : ObfuscatedIdentifiableController<DebitCard>
    {
        public DebitCardsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<DebitCard, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
