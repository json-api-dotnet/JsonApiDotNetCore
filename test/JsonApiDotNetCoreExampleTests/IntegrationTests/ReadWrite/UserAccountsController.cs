using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ReadWrite
{
    public sealed class UserAccountsController : JsonApiController<UserAccount, long>
    {
        public UserAccountsController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<UserAccount, long> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
