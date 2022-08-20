using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers;

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers;

public sealed partial class BedsController : JsonApiQueryController<Bed, int>
{
    public BedsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceQueryService<Bed, int> queryService)
        : base(options, resourceGraph, loggerFactory, queryService)
    {
    }
}
