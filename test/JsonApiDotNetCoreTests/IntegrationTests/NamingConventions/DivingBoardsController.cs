using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.NamingConventions
{
    public sealed class DivingBoardsController : JsonApiController<DivingBoard, int>
    {
        public DivingBoardsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<DivingBoard, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
