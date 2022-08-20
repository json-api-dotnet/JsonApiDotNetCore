using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.ReadWrite;

namespace JsonApiDotNetCoreTests.IntegrationTests.ReadWrite;

public sealed partial class UserAccountsController : JsonApiController<UserAccount, long>
{
    public UserAccountsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<UserAccount, long> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
