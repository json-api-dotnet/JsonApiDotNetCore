using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace OpenApiTests.NamingConventions
{
    public sealed class SupermarketsController : JsonApiController<Supermarket, int>
    {
        public SupermarketsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<Supermarket, int> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
