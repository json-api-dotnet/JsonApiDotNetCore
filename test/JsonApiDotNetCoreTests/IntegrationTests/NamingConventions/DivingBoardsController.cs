using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.NamingConventions
{
    public sealed class DivingBoardsController : JsonApiController<DivingBoard>
    {
        public DivingBoardsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<DivingBoard> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
