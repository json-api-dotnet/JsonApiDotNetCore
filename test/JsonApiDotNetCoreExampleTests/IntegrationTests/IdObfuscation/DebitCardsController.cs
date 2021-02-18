using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.IdObfuscation
{
    public sealed class DebitCardsController : ObfuscatedIdentifiableController<DebitCard>
    {
        public DebitCardsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<DebitCard> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
