using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.IdObfuscation
{
    public sealed class BankAccountsController : ObfuscatedIdentifiableController<BankAccount>
    {
        public BankAccountsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<BankAccount, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
