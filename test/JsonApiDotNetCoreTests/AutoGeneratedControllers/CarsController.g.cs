#nullable enable

using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.CompositeKeys;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompositeKeys;

public sealed partial class CarsController : JsonApiController<Car, string?>
{
    public CarsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Car, string?> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
