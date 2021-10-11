#nullable disable

using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.EagerLoading
{
    public sealed class StreetsController : JsonApiController<Street, int>
    {
        public StreetsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Street, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
