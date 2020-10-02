using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Controllers
{
    public sealed class AnimalsController : JsonApiController<Animal>
    {
        public AnimalsController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Animal> resourceService)
            : base(options, loggerFactory, resourceService) { }
    }
}
