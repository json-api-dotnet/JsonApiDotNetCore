using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ModelStateValidation
{
    public sealed class SystemFilesController : JsonApiController<SystemFile>
    {
        public SystemFilesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<SystemFile> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
