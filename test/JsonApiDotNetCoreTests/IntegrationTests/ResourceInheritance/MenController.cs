#nullable disable

using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance
{
    public sealed class MenController : JsonApiController<Man, int>
    {
        public MenController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Man, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
