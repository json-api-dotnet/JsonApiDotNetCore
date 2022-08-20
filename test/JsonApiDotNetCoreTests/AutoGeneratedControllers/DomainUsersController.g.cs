using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.Microservices;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices;

public sealed partial class DomainUsersController : JsonApiController<DomainUser, System.Guid>
{
    public DomainUsersController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<DomainUser, System.Guid> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
