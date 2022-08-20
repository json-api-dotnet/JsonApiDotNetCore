using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.Microservices;

namespace JsonApiDotNetCoreTests.IntegrationTests.Microservices;

public sealed partial class DomainGroupsController : JsonApiController<DomainGroup, System.Guid>
{
    public DomainGroupsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<DomainGroup, System.Guid> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
