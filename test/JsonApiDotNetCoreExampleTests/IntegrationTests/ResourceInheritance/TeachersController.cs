using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance
{
    public sealed class TeachersController : JsonApiController<Teacher>
    {
        public TeachersController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Teacher> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
