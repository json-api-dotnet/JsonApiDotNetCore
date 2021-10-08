using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.EagerLoading
{
    public sealed class StatesController : JsonApiController<State, int>
    {
        public StatesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<State> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
