using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    public sealed class MenController : JsonApiController<Man>
    {
        public MenController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Man> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
