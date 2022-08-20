using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceConstructorInjection;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceConstructorInjection;

public sealed partial class PostOfficesController : JsonApiController<PostOffice, int>
{
    public PostOfficesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<PostOffice, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
