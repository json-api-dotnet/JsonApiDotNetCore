using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.InputValidation.ModelState
{
    public sealed class SystemFilesController : JsonApiController<SystemFile, int>
    {
        public SystemFilesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<SystemFile> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
