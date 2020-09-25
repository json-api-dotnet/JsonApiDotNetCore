using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    public sealed class AnimalsController : JsonApiController<Animal>
    {
        public AnimalsController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Animal> resourceService)
            : base(options, loggerFactory, resourceService) { }
    }
}
