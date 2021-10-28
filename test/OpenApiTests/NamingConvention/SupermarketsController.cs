using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace OpenApiTests.NamingConvention
{
    public sealed class SupermarketsController : JsonApiController<Supermarket>
    {
        public SupermarketsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Supermarket> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
