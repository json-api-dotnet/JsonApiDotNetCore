using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.Archiving
{
    public sealed class TelevisionStationsController : JsonApiController<TelevisionStation, int>
    {
        public TelevisionStationsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<TelevisionStation> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
