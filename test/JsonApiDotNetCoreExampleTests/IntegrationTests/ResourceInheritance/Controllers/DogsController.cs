using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Controllers
{
    public sealed class DogsController : JsonApiController<Dog>
    {
        public DogsController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Dog> resourceService)
            : base(options, loggerFactory, resourceService) { }
    }
}
