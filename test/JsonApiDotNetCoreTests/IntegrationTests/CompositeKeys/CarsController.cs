#nullable disable

using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompositeKeys
{
    public sealed class CarsController : JsonApiController<Car, string>
    {
        public CarsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IResourceService<Car, string> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
