using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.IdObfuscation
{
    public sealed class BankAccountsController : ObfuscatedIdentifiableController<BankAccount>
    {
        public BankAccountsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<BankAccount> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
