using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.InputValidation.ModelState
{
    public sealed class SystemDirectoriesController : JsonApiController<SystemDirectory>
    {
        public SystemDirectoriesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<SystemDirectory> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
